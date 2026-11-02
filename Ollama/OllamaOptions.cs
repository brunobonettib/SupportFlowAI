namespace SupportFlowAI.Ollama;

public sealed class OllamaOptions
{
    public const string Endpoint = "http://127.0.0.1:11434/api/chat";
    public string Model { get; set; } = "gemma3:4b";
    public int TimeoutSeconds { get; set; } = 120;
    public int MaxOutputTokens { get; set; } = 1000;
    public int ContextTokens { get; set; } = 8192;

    // Somente tags locais simples; servidor iniciado com OLLAMA_NO_CLOUD=1.
    public bool IsValid() => !string.IsNullOrWhiteSpace(Model) && Model.Length <= 100 &&
        Model.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '.' or '_' or ':') &&
        !Model.Contains("cloud", StringComparison.OrdinalIgnoreCase) &&
        TimeoutSeconds is >= 1 and <= 300 && MaxOutputTokens is >= 128 and <= 4000 &&
        ContextTokens is >= 8192 and <= 16384;
}
