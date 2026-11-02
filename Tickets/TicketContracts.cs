namespace SupportFlowAI.Tickets;

public sealed record AnalyzeTicketRequest(string? Message);

public sealed record TicketAnalysis(
    string Categoria,
    string Prioridade,
    string Justificativa,
    string SugestaoResposta)
{
    // Definido pelo backend, nunca delegado ao modelo.
    public bool RequerRevisaoHumana => true;

    // Diagnóstico transitório usado apenas pelo avaliador de dados fictícios; nunca serializado pela API.
    [System.Text.Json.Serialization.JsonIgnore]
    public ModelTextDraft? ModelDraft { get; init; }
}

public sealed record ModelTextDraft(string Justificativa, string SugestaoResposta);

public static class TicketRules
{
    public const int MaxMessageLength = 4000;
    public static IReadOnlyList<string> Categories { get; } =
        Array.AsReadOnly(new[] { "Entrega", "Pagamento", "Cancelamento", "Reembolso", "Outro" });
    public static IReadOnlyList<string> Priorities { get; } =
        Array.AsReadOnly(new[] { "Baixa", "Media", "Alta" });

    public static string? ValidateMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "A mensagem é obrigatória.";
        if (message.Length > MaxMessageLength)
            return $"A mensagem deve ter no máximo {MaxMessageLength} caracteres.";
        return null;
    }
}

public interface ITicketAnalyzer
{
    Task<TicketAnalysis> AnalyzeAsync(string message, CancellationToken cancellationToken);
}
