using SupportFlowAI.Evaluation;
using Xunit;

namespace SupportFlowAI.Tests;

public class EvaluationPermissionTests
{
    [Theory]
    [InlineData("Ollama", false, false, true, true)]
    [InlineData("Ollama", true, true, false, false)]
    [InlineData("OpenAI", true, false, true, false)]
    [InlineData("OpenAI", true, true, false, true)]
    [InlineData("OpenAI", false, true, true, false)]
    [InlineData("Other", true, true, true, false)]
    public void Authorization_is_specific_to_provider(string provider, bool key, bool paid, bool local, bool allowed) =>
        Assert.Equal(allowed, EvaluationPermission.SkipReason(provider, key, paid, local) is null);

    [Fact]
    public void All_failures_are_in_denominators_and_latency()
    {
        var result = EvaluationMetrics.Calculate([
            new("a", "Outro", "Baixa", null, "provider_timeout", 200),
            new("b", "Entrega", "Media", null, "invalid_provider_response", 400)]);
        Assert.Equal(2, result.FailureCount);
        Assert.Equal(0d, result.CategoryAccuracy);
        Assert.Equal(0d, result.PriorityAccuracy);
        Assert.Equal(0d, result.ValidResponseRate);
        Assert.Equal(300d, result.AverageLatencyMs);
    }
}
