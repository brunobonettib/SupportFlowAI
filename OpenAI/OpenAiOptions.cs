namespace SupportFlowAI.OpenAI;

public sealed class OpenAiOptions
{
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "gpt-6-luna";
    public string ReasoningEffort { get; set; } = "none";
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxOutputTokens { get; set; } = 1000;
    public bool HasApiKey => !string.IsNullOrWhiteSpace(ApiKey);

    public bool IsValid() => !string.IsNullOrWhiteSpace(Model) && Model.Length <= 100 &&
        Model.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '.' or '_') &&
        new[] { "none", "minimal", "low", "medium", "high", "xhigh", "max" }.Contains(ReasoningEffort) &&
        TimeoutSeconds is >= 1 and <= 120 && MaxOutputTokens is >= 128 and <= 4000;
}
