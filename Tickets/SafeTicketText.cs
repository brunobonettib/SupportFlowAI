namespace SupportFlowAI.Tickets;

/// <summary>
/// Não tenta reconhecer alucinações por palavras-chave. Nenhum texto livre do LLM é publicado
/// pelo cliente local: usa formulações controladas, sem evidência operacional inventada.
/// Categoria/prioridade continuam sendo decisões do modelo, sujeitas à revisão humana.
/// </summary>
public static class SafeTicketText
{
    public const string Version = "1.0";

    public static TicketAnalysis Apply(TicketAnalysis draft)
    {
        if (!TicketRules.Categories.Contains(draft.Categoria) || !TicketRules.Priorities.Contains(draft.Prioridade))
            throw AnalysisException.InvalidResponse();
        var criterion = draft.Prioridade switch
        {
            "Alta" => "Alta indica possível risco, prejuízo ou janela urgente; confirme os sinais na revisão prioritária.",
            "Media" => "Media indica possível problema ativo; confirme a ausência de sinais de urgência na revisão.",
            _ => "Baixa indica possível dúvida geral ou falta de contexto; confirme se existe um problema ativo."
        };
        var suggestion = draft.Prioridade == "Alta"
            ? "Sua solicitação precisa de análise prioritária por um atendente para confirmar os detalhes e orientar os próximos passos."
            : draft.Categoria switch
            {
                "Entrega" => "Para orientar sobre a entrega, um atendente precisa esclarecer sua dúvida ou conferir os detalhes disponíveis. Não há confirmação de status nesta sugestão.",
                "Pagamento" => "Para orientar sobre pagamento, um atendente precisa conferir sua dúvida ou o problema relatado. Não envie senha ou dados completos do cartão.",
                "Cancelamento" => "Um atendente precisa esclarecer sua dúvida ou avaliar a possibilidade de cancelamento. Esta mensagem não confirma que o pedido foi cancelado.",
                "Reembolso" => "Um atendente precisa esclarecer sua dúvida ou avaliar a solicitação de reembolso. Não há confirmação de aprovação, estorno ou prazo nesta mensagem.",
                _ => "Pode explicar qual orientação você procura e se há algum problema acontecendo? Um atendente precisa desses detalhes para avaliar a solicitação."
            };
        return new(draft.Categoria, draft.Prioridade,
            $"Triagem preliminar em {draft.Categoria}. {criterion}", suggestion)
        {
            ModelDraft = new(draft.Justificativa, draft.SugestaoResposta)
        };
    }
}
