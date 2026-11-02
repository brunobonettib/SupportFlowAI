using System.Text.Json;

namespace SupportFlowAI.Tickets;

public static class TicketOutputParser
{
    public static TicketAnalysis Parse(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions { AllowDuplicateProperties = false });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                throw AnalysisException.InvalidResponse();

            string[] expected = ["categoria", "prioridade", "justificativa", "sugestaoResposta"];
            var names = root.EnumerateObject().Select(p => p.Name).ToArray();
            if (names.Length != expected.Length || names.Distinct().Count() != expected.Length ||
                names.Except(expected, StringComparer.Ordinal).Any())
                throw AnalysisException.InvalidResponse();

            var category = ReadString(root, "categoria", 30);
            var priority = ReadString(root, "prioridade", 10);
            if (!TicketRules.Categories.Contains(category, StringComparer.Ordinal) ||
                !TicketRules.Priorities.Contains(priority, StringComparer.Ordinal))
                throw AnalysisException.InvalidResponse();

            return new TicketAnalysis(category, priority,
                ReadString(root, "justificativa", 400),
                ReadString(root, "sugestaoResposta", 1200));
        }
        catch (JsonException)
        {
            throw AnalysisException.InvalidResponse();
        }
    }

    private static string ReadString(JsonElement root, string name, int maxLength)
    {
        var element = root.GetProperty(name);
        if (element.ValueKind != JsonValueKind.String)
            throw AnalysisException.InvalidResponse();
        var value = element.GetString();
        if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength)
            throw AnalysisException.InvalidResponse();
        return value;
    }
}
