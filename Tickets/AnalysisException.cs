namespace SupportFlowAI.Tickets;

// Somente mensagens fixas e seguras: nunca repassar o corpo do provedor.
public sealed class AnalysisException(string code, int statusCode, string message)
    : Exception(message)
{
    public string Code { get; } = code;
    public int StatusCode { get; } = statusCode;

    public static AnalysisException InvalidResponse() => new(
        "invalid_provider_response", 502, "O provedor retornou uma resposta inválida. Tente novamente.");
}
