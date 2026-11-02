using SupportFlowAI.Evaluation;
using SupportFlowAI.Tickets;
using Xunit;

namespace SupportFlowAI.Tests;

public class EvaluationMetricsTests
{
    [Fact]
    public void No_execution_does_not_create_fake_metrics()
    {
        var metrics = EvaluationMetrics.Calculate([]);
        Assert.Equal(0, metrics.Attempted);
        Assert.Null(metrics.CategoryAccuracy);
        Assert.Null(metrics.PriorityAccuracy);
        Assert.Null(metrics.ValidResponseRate);
        Assert.Null(metrics.AverageLatencyMs);
    }

    [Fact]
    public void Errors_stay_in_denominator_and_latency_includes_failures()
    {
        var metrics = EvaluationMetrics.Calculate([
            new("1", "Entrega", "Media", new TicketAnalysis("Entrega", "Alta", "Teste", "Teste"), null, 100),
            new("2", "Outro", "Baixa", new TicketAnalysis("Outro", "Baixa", "Teste", "Teste"), null, 200),
            new("3", "Pagamento", "Alta", null, "provider_timeout", 300)
        ]);
        Assert.Equal(3, metrics.Attempted);
        Assert.Equal(2d / 3, metrics.CategoryAccuracy);
        Assert.Equal(1d / 3, metrics.PriorityAccuracy);
        Assert.Equal(2d / 3, metrics.ValidResponseRate);
        Assert.Equal(200d, metrics.AverageLatencyMs);
    }
}
