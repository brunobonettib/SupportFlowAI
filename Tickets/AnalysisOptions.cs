namespace SupportFlowAI.Tickets;

public sealed class AnalysisOptions
{
    public string Provider { get; set; } = "Ollama";
    public bool IsValid() => Provider is "Ollama" or "OpenAI";
}
