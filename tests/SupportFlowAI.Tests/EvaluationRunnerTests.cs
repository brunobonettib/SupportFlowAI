using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SupportFlowAI.Evaluation;
using SupportFlowAI.Ollama;
using SupportFlowAI.OpenAI;
using SupportFlowAI.Tickets;
using Xunit;

namespace SupportFlowAI.Tests;

public class EvaluationRunnerTests
{
    [Fact]
    public async Task Local_flag_cannot_enable_paid_provider()
    {
        var root = CreateRoot();
        var analyzer = new Stub();
        using var services = Services("OpenAI", analyzer);
        Assert.Equal(2, await EvaluationRunner.RunAsync(services, root, false, true));
        using var report = Report(root);
        Assert.Equal(0, analyzer.Calls);
        Assert.Equal("nao_executado", report.RootElement.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, report.RootElement.GetProperty("metrics").GetProperty("categoryAccuracy").ValueKind);
    }

    [Fact]
    public async Task Reports_keep_provider_failures_and_correct_denominator()
    {
        var root = CreateRoot();
        var analyzer = new Stub();
        using var services = Services("Ollama", analyzer);
        Assert.Equal(1, await EvaluationRunner.RunAsync(services, root, false, true));
        using var report = Report(root);
        Assert.Equal("Ollama", report.RootElement.GetProperty("provider").GetString());
        var metrics = report.RootElement.GetProperty("metrics");
        Assert.Equal(15, metrics.GetProperty("attempted").GetInt32());
        Assert.Equal(1, metrics.GetProperty("failureCount").GetInt32());
        Assert.Equal(14d / 15, metrics.GetProperty("validResponseRate").GetDouble());
        Assert.Equal(15, analyzer.Calls);
    }

    [Fact]
    public async Task Cancellation_writes_partial_report_and_stops_remaining_calls()
    {
        var root = CreateRoot();
        using var cancel = new CancellationTokenSource();
        var analyzer = new Stub(cancel);
        using var services = Services("Ollama", analyzer);
        Assert.Equal(1, await EvaluationRunner.RunAsync(services, root, false, true, cancel.Token));
        using var report = Report(root);
        Assert.Equal("interrompido", report.RootElement.GetProperty("status").GetString());
        Assert.Equal(1, analyzer.Calls);
        Assert.Equal(1, report.RootElement.GetProperty("metrics").GetProperty("attempted").GetInt32());
    }

    private static ServiceProvider Services(string provider, Stub analyzer) => new ServiceCollection()
        .AddSingleton<IOptions<AnalysisOptions>>(Options.Create(new AnalysisOptions { Provider = provider }))
        .AddSingleton<IOptions<OpenAiOptions>>(Options.Create(new OpenAiOptions { ApiKey = "fake-test-key" }))
        .AddSingleton<IOptions<OllamaOptions>>(Options.Create(new OllamaOptions()))
        .AddSingleton<ITicketAnalyzer>(analyzer).BuildServiceProvider();

    [Fact]
    public async Task Warmup_is_separate_from_fifteen_measured_cases()
    {
        var root = CreateRoot();
        var analyzer = new Stub(failFirst: false);
        using var services = Services("Ollama", analyzer);
        Assert.Equal(0, await EvaluationRunner.RunAsync(services, root, false, true, warmUp: true));
        using var report = Report(root);
        Assert.Equal(16, analyzer.Calls);
        Assert.Equal(15, report.RootElement.GetProperty("metrics").GetProperty("attempted").GetInt32());
        Assert.True(report.RootElement.GetProperty("warmup").GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task Failed_warmup_does_not_start_the_dataset()
    {
        var root = CreateRoot();
        var analyzer = new Stub();
        using var services = Services("Ollama", analyzer);
        Assert.Equal(2, await EvaluationRunner.RunAsync(services, root, false, true, warmUp: true));
        using var report = Report(root);
        Assert.Equal(1, analyzer.Calls);
        Assert.Equal(0, report.RootElement.GetProperty("metrics").GetProperty("attempted").GetInt32());
        Assert.False(report.RootElement.GetProperty("warmup").GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task Holdout_selection_uses_the_distinct_dataset()
    {
        var root = CreateRoot();
        File.Copy(Path.Combine(AppContext.BaseDirectory, "holdout.json"), Path.Combine(root, "evaluation", "holdout.json"));
        using var services = Services("Ollama", new Stub(failFirst: false));
        Assert.Equal(0, await EvaluationRunner.RunAsync(services, root, false, true, evaluationSet: "holdout"));
        using var report = Report(root);
        Assert.Equal("holdout", report.RootElement.GetProperty("evaluationSet").GetString());
        Assert.Equal("H01", report.RootElement.GetProperty("cases")[0].GetProperty("id").GetString());
    }

    [Fact]
    public async Task Warmup_never_adds_a_paid_call_even_with_paid_flag()
    {
        var root = CreateRoot();
        var analyzer = new Stub();
        using var services = Services("OpenAI", analyzer);
        Assert.Equal(2, await EvaluationRunner.RunAsync(services, root, true, warmUp: true));
        Assert.Equal(0, analyzer.Calls);
    }

    private static string CreateRoot()
    {
        // Artefatos só dentro da saída do projeto de testes, nunca em Temp global.
        var root = Path.Combine(AppContext.BaseDirectory, "evaluation-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "evaluation"));
        File.Copy(Path.Combine(AppContext.BaseDirectory, "dataset.json"), Path.Combine(root, "evaluation", "dataset.json"));
        return root;
    }
    private static JsonDocument Report(string root) =>
        JsonDocument.Parse(File.ReadAllText(Directory.GetFiles(Path.Combine(root, "evaluation", "reports"), "*.json").Single()));

    private sealed class Stub(CancellationTokenSource? cancel = null, bool failFirst = true) : ITicketAnalyzer
    {
        public int Calls { get; private set; }
        public Task<TicketAnalysis> AnalyzeAsync(string message, CancellationToken token)
        {
            Calls++;
            if (cancel is not null) { cancel.Cancel(); token.ThrowIfCancellationRequested(); }
            if (Calls == 1 && failFirst) throw new AnalysisException("provider_timeout", 504, "Falha fictícia.");
            return Task.FromResult(new TicketAnalysis("Entrega", "Media", "Teste fictício.", "Sugestão fictícia."));
        }
    }
}
