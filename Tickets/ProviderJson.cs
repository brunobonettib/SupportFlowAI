using System.Text.Json;

namespace SupportFlowAI.Tickets;

public static class ProviderJson
{
    public const int MaxResponseBytes = 65536;

    public static async Task<JsonDocument> ReadAsync(HttpContent content, CancellationToken token)
    {
        await using var stream = await content.ReadAsStreamAsync(token);
        using var buffer = new MemoryStream();
        var bytes = new byte[8192];
        int count;
        while ((count = await stream.ReadAsync(bytes, token)) > 0)
        {
            if (buffer.Length + count > MaxResponseBytes)
                throw AnalysisException.InvalidResponse();
            buffer.Write(bytes, 0, count);
        }
        return JsonDocument.Parse(buffer.ToArray(), new JsonDocumentOptions { AllowDuplicateProperties = false });
    }
}
