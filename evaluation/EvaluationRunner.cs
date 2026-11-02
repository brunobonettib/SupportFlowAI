using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SupportFlowAI.OpenAI;
using SupportFlowAI.Ollama;
using SupportFlowAI.Tickets;

namespace SupportFlowAI.Evaluation;

public static class EvaluationRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(IServiceProvider services, string root, bool allowPaid,
        bool allowLocal = false, CancellationToken cancellationToken = default, string evaluationSet = "original", bool warmUp = false)
    {
        var provider = services.GetRequiredService<IOptions<AnalysisOptions>>().Value.Provider;
        var local = provider == "Ollama";
        var localOptions = local ? services.GetRequiredService<IOptions<OllamaOptions>>().Value : null;
        var options = local ? null : services.GetRequiredService<IOptions<OpenAiOptions>>().Value;
        var model = local ? localOptions!.Model : options!.Model;
        var timeout = local ? localOptions!.TimeoutSeconds : options!.TimeoutSeconds;
        var maxTokens = local ? localOptions!.MaxOutputTokens : options!.MaxOutputTokens;
        var effort = local ? "não aplicável; temperatura 0, seed 42" : options!.ReasoningEffort;
        var datasetFile = evaluationSet switch
        {
            "original" => "dataset.json",
            "holdout" => "holdout.json",
            _ => throw new ArgumentException("Conjunto deve ser original ou holdout.", nameof(evaluationSet))
        };
        var datasetBytes = await File.ReadAllBytesAsync(Path.Combine(root, "evaluation", datasetFile));
        var cases = JsonSerializer.Deserialize<EvaluationCase[]>(datasetBytes, JsonOptions)
            ?? throw new InvalidOperationException("Dataset vazio.");
        if (cases.Length != 15 || cases.Any(c => c is null || string.IsNullOrWhiteSpace(c.Id) ||
                TicketRules.ValidateMessage(c.Message) is not null ||
                !TicketRules.Categories.Contains(c.Categoria) || !TicketRules.Priorities.Contains(c.Prioridade)) ||
            cases.Select(c => c.Id).Distinct().Count() != cases.Length)
            throw new InvalidOperationException("Dataset inválido: são necessários 15 casos únicos com rótulos válidos.");

        var rows = new List<EvaluationRow>();
        var skipReason = EvaluationPermission.SkipReason(provider, options?.HasApiKey ?? false, allowPaid, allowLocal);
        var interrupted = false;
        object? warmupResult = null;
        var started = DateTimeOffset.UtcNow;
        if (warmUp && !local) skipReason = "Aquecimento automático permitido somente no Ollama local.";
        if (skipReason is null && warmUp)
        {
            var timer = Stopwatch.StartNew();
            string? warmupError = null;
            try
            {
                await services.GetRequiredService<ITicketAnalyzer>().AnalyzeAsync("Olá, gostaria de orientação geral.", cancellationToken);
            }
            catch (AnalysisException error) { warmupError = error.Code; }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { warmupError = "evaluation_cancelled"; }
            timer.Stop();
            warmupResult = new { success = warmupError is null, errorCode = warmupError, latencyMs = timer.Elapsed.TotalMilliseconds };
            if (warmupError is not null) skipReason = $"Aquecimento falhou: {warmupError}. Lote não iniciado.";
            Console.WriteLine($"Aquecimento: {warmupError ?? "concluído"} ({timer.ElapsedMilliseconds} ms); excluído das métricas.");
        }
        if (skipReason is null)
        {
            var analyzer = services.GetRequiredService<ITicketAnalyzer>();
            foreach (var item in cases)
            {
                if (cancellationToken.IsCancellationRequested) { interrupted = true; break; }
                var stopwatch = Stopwatch.StartNew();
                TicketAnalysis? analysis = null;
                string? code = null;
                try
                {
                    analysis = await analyzer.AnalyzeAsync(item.Message, cancellationToken);
                }
                catch (AnalysisException error)
                {
                    code = error.Code;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    code = "evaluation_cancelled";
                    interrupted = true;
                }
                stopwatch.Stop();
                rows.Add(new(item.Id, item.Categoria, item.Prioridade, analysis, code, stopwatch.Elapsed.TotalMilliseconds)
                { ModelDraft = analysis?.ModelDraft });
                Console.WriteLine($"{item.Id}: {(code ?? "resposta válida")} ({stopwatch.ElapsedMilliseconds} ms)");
                if (interrupted) break;
            }
        }

        var metrics = EvaluationMetrics.Calculate(rows);
        var report = new
        {
            status = skipReason is not null ? "nao_executado" : interrupted ? "interrompido" : "executado",
            reason = skipReason,
            startedAtUtc = started,
            completedAtUtc = DateTimeOffset.UtcNow,
            provider,
            evaluationSet,
            warmup = warmupResult,
            textPolicyVersion = local ? SafeTicketText.Version : null,
            textOrigin = local ? "formulações controladas pelo backend; rascunhos separados" : "modelo",
            modelConfigured = model,
            reasoningEffort = effort,
            timeoutSeconds = timeout,
            maxOutputTokens = maxTokens,
            contextTokens = localOptions?.ContextTokens,
            promptVersion = TicketPrompt.Version,
            promptSha256 = Hash(Encoding.UTF8.GetBytes(TicketPrompt.Instructions)),
            schemaSha256 = Hash(JsonSerializer.SerializeToUtf8Bytes(TicketPrompt.Schema)),
            datasetSha256 = Hash(datasetBytes),
            datasetSize = cases.Length,
            metrics,
            cases = rows,
            limitations = new[]
            {
                "15 exemplos sintéticos preparados com apoio do Codex; não representam tráfego real nem rotulagem independente.",
                "Latência mede o serviço de análise (rede, provedor e validação); não mede o endpoint HTTP nem trabalho humano.",
                "Falhas contam no denominador das acurácias. Validade estrutural não garante correção semântica.",
                "Sugestões precisam de revisão humana. Economia de tempo, custo real e impacto operacional não foram medidos.",
                "Modelo configurado pode ser um alias mutável; resultados podem variar entre execuções."
            }
        };
        var folder = Path.Combine(root, "evaluation", "reports");
        Directory.CreateDirectory(folder);
        var name = $"evaluation-{started:yyyyMMdd-HHmmss-fff}-{Guid.NewGuid():N}";
        var jsonPath = Path.Combine(folder, name + ".json");
        var markdownPath = Path.Combine(folder, name + ".md");
        await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(report, JsonOptions));

        var markdown = new StringBuilder();
        markdown.AppendLine("# Avaliação SupportFlow AI");
        markdown.AppendLine();
        markdown.AppendLine($"Início (UTC): {started:O} | Provedor: {provider} | Modelo configurado: `{model}` | Prompt: {TicketPrompt.Version}");
        markdown.AppendLine($"Status: **{report.status}**. {skipReason}");
        markdown.AppendLine($"Dataset: {cases.Length} mensagens fictícias; SHA-256: `{report.datasetSha256}`.");
        markdown.AppendLine($"Conjunto: {evaluationSet}. Aquecimento separado solicitado: {warmUp}. Origem do texto público: {report.textOrigin}.");
        markdown.AppendLine($"Esforço: {effort}; limite: {maxTokens} tokens; timeout: {timeout}s; sem retry.");
        markdown.AppendLine();
        markdown.AppendLine("| Métrica | Resultado |");
        markdown.AppendLine("|---|---|");
        markdown.AppendLine($"| Chamadas executadas | {metrics.Attempted} |");
        markdown.AppendLine($"| Falhas | {metrics.FailureCount} |");
        markdown.AppendLine($"| Acurácia de categoria | {Percent(metrics.CategoryAccuracy)} ({metrics.CategoryCorrect}/{metrics.Attempted}) |");
        markdown.AppendLine($"| Acurácia de prioridade | {Percent(metrics.PriorityAccuracy)} ({metrics.PriorityCorrect}/{metrics.Attempted}) |");
        markdown.AppendLine($"| Respostas válidas | {Percent(metrics.ValidResponseRate)} ({metrics.Valid}/{metrics.Attempted}) |");
        markdown.AppendLine($"| Recall de Alta | {Percent(metrics.HighPriorityRecall)} |");
        markdown.AppendLine($"| Latência média, todas as tentativas | {(metrics.AverageLatencyMs?.ToString("F1", CultureInfo.InvariantCulture) ?? "não medida")} ms |");
        markdown.AppendLine();
        markdown.AppendLine("| Caso | Categoria esperada → obtida | Prioridade esperada → obtida | Resultado | ms |");
        markdown.AppendLine("|---|---|---|---|---|");
        foreach (var row in rows)
        {
            var result = row.ErrorCode ?? (row.Analysis?.Categoria == row.ExpectedCategory &&
                row.Analysis?.Prioridade == row.ExpectedPriority ? "correto" : "divergência");
            markdown.AppendLine($"| {row.Id} | {row.ExpectedCategory} → {row.Analysis?.Categoria ?? "—"} | {row.ExpectedPriority} → {row.Analysis?.Prioridade ?? "—"} | {result} | {row.LatencyMs.ToString("F1", CultureInfo.InvariantCulture)} |");
        }
        markdown.AppendLine();
        AppendClassification(markdown, "Categoria", metrics.Categories!);
        AppendClassification(markdown, "Prioridade", metrics.Priorities!);
        foreach (var limitation in report.limitations)
            markdown.AppendLine($"- {limitation}");
        markdown.AppendLine();
        markdown.AppendLine("O JSON irmão separa texto entregue e rascunho do modelo para revisão dos dados fictícios; não contém chave nem corpos brutos de erro. Invenções exigem revisão contextual, não são detectadas por estas métricas de classificação.");
        markdown.AppendLine(local
            ? $"Reproduzir: `scripts\\Avaliar-Local.cmd {evaluationSet}` (Ollama local; sem cobrança de API)."
            : "Reproduzir somente após estimativa e autorização: selecionar Analysis:Provider=OpenAI e usar --evaluate --allow-paid-evaluation (gera custo).");
        await File.WriteAllTextAsync(markdownPath, markdown.ToString());
        Console.WriteLine($"Relatório: {markdownPath}");
        return skipReason is not null ? 2 : interrupted || rows.Any(r => r.ErrorCode is not null) ? 1 : 0;
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static void AppendClassification(StringBuilder markdown, string name, ClassificationMetrics metrics)
    {
        markdown.AppendLine($"### {name}: precisão/recall por classe");
        markdown.AppendLine("| Classe | Suporte | Predições | Precisão | Recall |");
        markdown.AppendLine("|---|---|---|---|---|");
        foreach (var (label, score) in metrics.ByClass)
            markdown.AppendLine($"| {label} | {score.Support} | {score.Predicted} | {Percent(score.Precision)} | {Percent(score.Recall)} |");
        var columns = metrics.ByClass.Keys.Append(ClassificationMetrics.NoResponse).ToArray();
        markdown.AppendLine();
        markdown.AppendLine("Matriz: linhas esperadas, colunas obtidas.");
        markdown.AppendLine("| Esperada | " + string.Join(" | ", columns) + " |");
        markdown.AppendLine("|---|" + string.Join("|", columns.Select(_ => "---")) + "|");
        foreach (var (label, values) in metrics.ConfusionMatrix)
            markdown.AppendLine($"| {label} | " + string.Join(" | ", columns.Select(c => values[c])) + " |");
        markdown.AppendLine();
    }
    private static string Percent(double? value) => value is null ? "não medida" :
        (value.Value * 100).ToString("F2", CultureInfo.InvariantCulture) + "%";
}
