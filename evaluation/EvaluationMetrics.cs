using SupportFlowAI.Tickets;

namespace SupportFlowAI.Evaluation;

public sealed record EvaluationCase(string Id, string Message, string Categoria, string Prioridade);
public sealed record EvaluationRow(string Id, string ExpectedCategory, string ExpectedPriority,
    TicketAnalysis? Analysis, string? ErrorCode, double LatencyMs)
{
    // Só aparece no relatório de avaliação com dados fictícios, nunca no contrato HTTP.
    public ModelTextDraft? ModelDraft { get; init; }
}

public sealed record EvaluationMetrics(int Attempted, int Valid, int CategoryCorrect, int PriorityCorrect,
    double? CategoryAccuracy, double? PriorityAccuracy, double? ValidResponseRate, double? AverageLatencyMs)
{
    public int FailureCount => Attempted - Valid;
    public ClassificationMetrics? Categories { get; init; }
    public ClassificationMetrics? Priorities { get; init; }
    public double? HighPriorityRecall => Priorities?.ByClass["Alta"].Recall;

    public static EvaluationMetrics Calculate(IReadOnlyList<EvaluationRow> rows)
    {
        var valid = rows.Count(r => r.Analysis is not null && r.ErrorCode is null);
        var categories = rows.Count(r => r.ErrorCode is null && r.Analysis?.Categoria == r.ExpectedCategory);
        var priorities = rows.Count(r => r.ErrorCode is null && r.Analysis?.Prioridade == r.ExpectedPriority);
        // Falhas contam como erros nas duas acurácias; não selecionar apenas os sucessos.
        return new(rows.Count, valid, categories, priorities,
            rows.Count == 0 ? null : (double)categories / rows.Count,
            rows.Count == 0 ? null : (double)priorities / rows.Count,
            rows.Count == 0 ? null : (double)valid / rows.Count,
            rows.Count == 0 ? null : rows.Average(r => r.LatencyMs))
        {
            Categories = ClassificationMetrics.Calculate(rows, TicketRules.Categories, r => r.ExpectedCategory, r => r.Analysis?.Categoria),
            Priorities = ClassificationMetrics.Calculate(rows, TicketRules.Priorities, r => r.ExpectedPriority, r => r.Analysis?.Prioridade)
        };
    }
}
