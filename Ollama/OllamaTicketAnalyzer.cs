using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SupportFlowAI.OpenAI;
using SupportFlowAI.Tickets;

namespace SupportFlowAI.Ollama;

public sealed class OllamaTicketAnalyzer(HttpClient client, IOptions<OllamaOptions> options) : ITicketAnalyzer
{
    private readonly OllamaOptions settings = options.Value;

    public async Task<TicketAnalysis> AnalyzeAsync(string message, CancellationToken cancellationToken)
    {
        if (TicketRules.ValidateMessage(message) is not null)
            throw new AnalysisException("invalid_message", 400, "A mensagem é inválida.");
        if (!settings.IsValid())
            throw new AnalysisException("invalid_configuration", 503, "A configuração do modelo local é inválida.");

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(settings.TimeoutSeconds));
        using var request = new HttpRequestMessage(HttpMethod.Post, OllamaOptions.Endpoint)
        {
            Content = JsonContent.Create(new
            {
                model = settings.Model,
                messages = new[]
                {
                    new { role = "system", content = TicketPrompt.Instructions },
                    new { role = "user", content = message }
                },
                stream = false,
                format = TicketPrompt.Schema,
                options = new { temperature = 0, seed = 42, num_predict = settings.MaxOutputTokens, num_ctx = settings.ContextTokens },
                keep_alive = "5m"
            })
        };
        try
        {
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if (!response.IsSuccessStatusCode)
                throw response.StatusCode switch
                {
                    HttpStatusCode.NotFound => new AnalysisException("local_model_missing", 503,
                        "Modelo local não encontrado. Baixe o modelo configurado no Ollama."),
                    HttpStatusCode.TooManyRequests or >= HttpStatusCode.InternalServerError =>
                        new AnalysisException("local_provider_unavailable", 503, "Ollama indisponível ou ocupado."),
                    _ => new AnalysisException("local_request_rejected", 502, "Ollama rejeitou a solicitação. Verifique a configuração.")
                };
            using var document = await ProviderJson.ReadAsync(response.Content, timeout.Token);
            var root = document.RootElement;
            if (!root.GetProperty("done").GetBoolean() || root.GetProperty("done_reason").GetString() != "stop")
                throw AnalysisException.InvalidResponse();
            var output = root.GetProperty("message");
            if (output.GetProperty("role").GetString() != "assistant")
                throw AnalysisException.InvalidResponse();
            var text = output.GetProperty("content").GetString();
            return text is null ? throw AnalysisException.InvalidResponse() : SafeTicketText.Apply(TicketOutputParser.Parse(text));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AnalysisException("provider_timeout", 504, "O modelo local excedeu o tempo limite.");
        }
        catch (Exception error) when (error is HttpRequestException or IOException)
        {
            throw new AnalysisException("local_provider_unavailable", 503, "Inicie o Ollama local em 127.0.0.1:11434.");
        }
        catch (Exception error) when (error is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            throw AnalysisException.InvalidResponse();
        }
    }
}
