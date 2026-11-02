using SupportFlowAI.Evaluation;
using SupportFlowAI.Tickets;
using Xunit;

namespace SupportFlowAI.Tests;

public class ClassificationMetricsTests
{
    [Fact]
    public void Recall_precision_and_confusion_include_failed_high_priority_cases()
    {
        var metrics = EvaluationMetrics.Calculate([
            Row("1", "Alta", "Alta"), Row("2", "Alta", "Media"), Row("3", "Baixa", "Alta"),
            new("4", "Outro", "Alta", null, "provider_timeout", 100)]);
        Assert.Equal(1d / 3, metrics.HighPriorityRecall);
        var high = metrics.Priorities!.ByClass["Alta"];
        Assert.Equal(0.5, high.Precision);
        Assert.Equal(2, high.FalseNegative);
        Assert.Equal(1, high.FalsePositive);
        Assert.Equal(1, metrics.Priorities.ConfusionMatrix["Alta"][ClassificationMetrics.NoResponse]);
        Assert.Equal(1, metrics.Priorities.ConfusionMatrix["Alta"]["Media"]);
        Assert.Equal(4, metrics.Priorities.ConfusionMatrix.Values.Sum(row => row.Values.Sum()));
    }

    [Fact]
    public void Undefined_precision_or_recall_is_null_not_fabricated_zero()
    {
        var metrics = EvaluationMetrics.Calculate([Row("1", "Baixa", "Baixa")]);
        Assert.Null(metrics.Priorities!.ByClass["Alta"].Precision);
        Assert.Null(metrics.HighPriorityRecall);
        Assert.Null(EvaluationMetrics.Calculate([]).HighPriorityRecall);
    }

    private static EvaluationRow Row(string id, string expected, string predicted) =>
        new(id, "Outro", expected, new TicketAnalysis("Outro", predicted, "Fictício", "Fictício"), null, 100);
}
