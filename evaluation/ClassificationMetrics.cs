namespace SupportFlowAI.Evaluation;

public sealed record ClassScore(int Support, int Predicted, int TruePositive, int FalsePositive, int FalseNegative,
    double? Precision, double? Recall);

public sealed record ClassificationMetrics(Dictionary<string, ClassScore> ByClass,
    Dictionary<string, Dictionary<string, int>> ConfusionMatrix)
{
    public const string NoResponse = "SemResposta";

    // Linhas = referência; colunas = predição. Falhas são uma coluna explícita, nunca descartadas.
    public static ClassificationMetrics Calculate(IReadOnlyList<EvaluationRow> rows, IReadOnlyList<string> labels,
        Func<EvaluationRow, string> expected, Func<EvaluationRow, string?> predicted)
    {
        var matrix = labels.ToDictionary(label => label,
            _ => labels.Append(NoResponse).ToDictionary(label => label, _ => 0));
        foreach (var row in rows)
        {
            var prediction = row.ErrorCode is null ? predicted(row) : null;
            matrix[expected(row)][prediction is not null && labels.Contains(prediction) ? prediction : NoResponse]++;
        }
        var scores = labels.ToDictionary(label => label, label =>
        {
            var support = matrix[label].Values.Sum();
            var predictedCount = matrix.Values.Sum(columns => columns[label]);
            var truePositive = matrix[label][label];
            return new ClassScore(support, predictedCount, truePositive, predictedCount - truePositive, support - truePositive,
                predictedCount == 0 ? null : (double)truePositive / predictedCount,
                support == 0 ? null : (double)truePositive / support);
        });
        return new(scores, matrix);
    }
}
