namespace SupportFlowAI.Evaluation;

public static class EvaluationPermission
{
    public static string? SkipReason(string provider, bool hasKey, bool allowPaid, bool allowLocal) => provider switch
    {
        "Ollama" => allowLocal ? null : "Avaliação local não habilitada: use --allow-local-evaluation.",
        "OpenAI" when !hasKey => "Chave de API não configurada.",
        "OpenAI" => allowPaid ? null : "Execução paga não autorizada: use --allow-paid-evaluation conscientemente.",
        _ => "Provedor inválido."
    };
}
