using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SupportFlowAI.Tickets;

namespace SupportFlowAI.OpenAI;

public sealed class OpenAiTicketAnalyzer(HttpClient httpClient, IOptions<OpenAiOptions> options) : ITicketAnalyzer
{
    private readonly OpenAiOptions settings = options.Value;

    public async Task<TicketAnalysis> AnalyzeAsync(string message, CancellationToken cancellationToken)
    {
        if (TicketRules.ValidateMessage(message) is not null)
            throw new AnalysisException("invalid_message", 400, "A mensagem é inválida.");
        if (!settings.IsValid())
            throw new AnalysisException("invalid_configuration", 503, "A configuração do provedor é inválida.");
        if (!settings.HasApiKey)
            throw new AnalysisException("provider_not_configured", 503,
                "Configure OpenAI__ApiKey ou OPENAI_API_KEY no ambiente, ou OpenAI:ApiKey em user-secrets.");

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(settings.TimeoutSeconds));
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        request.Content = JsonContent.Create(new
        {
            model = settings.Model,
            instructions = TicketPrompt.Instructions,
            input = new[] { new { role = "user", content = message } },
            reasoning = new { effort = settings.ReasoningEffort },
            max_output_tokens = settings.MaxOutputTokens,
            store = false,
            text = new { format = new { type = "json_schema", name = "ticket_analysis", strict = true, schema = TicketPrompt.Schema } }
        });

        try
        {
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            if (!response.IsSuccessStatusCode)
                throw MapProviderError(response.StatusCode);
            using var document = await ProviderJson.ReadAsync(response.Content, timeout.Token);
            return ParseResponse(document.RootElement);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AnalysisException("provider_timeout", 504, "O provedor excedeu o tempo limite. Tente novamente.");
        }
        catch (HttpRequestException)
        {
            throw new AnalysisException("provider_unavailable", 503, "O provedor está indisponível. Tente novamente mais tarde.");
        }
        catch (IOException)
        {
            throw new AnalysisException("provider_unavailable", 503, "A conexão com o provedor foi interrompida.");
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            throw AnalysisException.InvalidResponse();
        }
    }

    private static TicketAnalysis ParseResponse(JsonElement root)
    {
        if (root.GetProperty("status").GetString() != "completed")
            throw AnalysisException.InvalidResponse();
        string? outputText = null;
        var messageCount = 0;
        foreach (var item in root.GetProperty("output").EnumerateArray())
        {
            if (item.GetProperty("type").GetString() != "message")
                continue;
            if (++messageCount > 1)
                throw AnalysisException.InvalidResponse();
            if (item.GetProperty("role").GetString() != "assistant" ||
                item.GetProperty("status").GetString() != "completed")
                throw AnalysisException.InvalidResponse();
            foreach (var content in item.GetProperty("content").EnumerateArray())
            {
                var type = content.GetProperty("type").GetString();
                if (type == "refusal")
                    throw new AnalysisException("provider_refusal", 502, "O provedor não produziu uma análise para esta mensagem.");
                if (type != "output_text" || outputText is not null)
                    throw AnalysisException.InvalidResponse();
                outputText = content.GetProperty("text").GetString();
            }
        }
        return outputText is null ? throw AnalysisException.InvalidResponse() : TicketOutputParser.Parse(outputText);
    }

    private static AnalysisException MapProviderError(HttpStatusCode status) => status switch
    {
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new(
            "provider_authentication_failed", 503, "Verifique as credenciais e permissões da API no servidor."),
        HttpStatusCode.TooManyRequests => new(
            "provider_rate_limited", 503, "O provedor atingiu um limite de uso ou de créditos. Tente mais tarde."),
        HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => new(
            "provider_timeout", 504, "O provedor excedeu o tempo limite. Tente novamente."),
        >= HttpStatusCode.InternalServerError => new(
            "provider_unavailable", 503, "O provedor está indisponível. Tente novamente mais tarde."),
        _ => new("provider_request_rejected", 502, "O provedor rejeitou a solicitação. Verifique modelo e configuração no servidor.")
    };
}
