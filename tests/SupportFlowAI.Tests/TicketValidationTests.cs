using System.Text.Json;
using SupportFlowAI.Tickets;
using Xunit;

namespace SupportFlowAI.Tests;

public class TicketValidationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\n")]
    public void Empty_messages_are_rejected(string? message) =>
        Assert.NotNull(TicketRules.ValidateMessage(message));

    [Theory]
    [InlineData(1, true)]
    [InlineData(4000, true)]
    [InlineData(4001, false)]
    public void Message_length_is_bounded(int length, bool valid) =>
        Assert.Equal(valid, TicketRules.ValidateMessage(new string('a', length)) is null);

    [Theory]
    [InlineData("Entrega", "Media")]
    [InlineData("Pagamento", "Alta")]
    [InlineData("Cancelamento", "Baixa")]
    [InlineData("Reembolso", "Media")]
    [InlineData("Outro", "Baixa")]
    public void Allowed_labels_are_mapped_without_changes(string category, string priority)
    {
        var analysis = TicketOutputParser.Parse(Output(category, priority));
        Assert.Equal(category, analysis.Categoria);
        Assert.Equal(priority, analysis.Prioridade);
        Assert.Equal("Solicitação fictícia.", analysis.Justificativa);
        Assert.Equal("Podemos verificar o pedido.", analysis.SugestaoResposta);
        Assert.True(analysis.RequerRevisaoHumana);
    }

    [Theory]
    [InlineData("VIP", "Alta")]
    [InlineData("entrega", "Media")]
    [InlineData("Entrega", "Média")]
    [InlineData("Entrega", "Urgente")]
    [InlineData(" Entrega", "Media")]
    public void Unexpected_labels_are_rejected(string category, string priority) =>
        AssertInvalid(Output(category, priority));

    [Theory]
    [InlineData("not json")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("{\"categoria\":\"Entrega\"}")]
    [InlineData("{\"categoria\":\"Entrega\",\"prioridade\":1,\"justificativa\":\"Teste\",\"sugestaoResposta\":\"Teste\"}")]
    [InlineData("{\"categoria\":\"Entrega\",\"prioridade\":\"Media\",\"justificativa\":null,\"sugestaoResposta\":\"Teste\"}")]
    [InlineData("{\"categoria\":\"Entrega\",\"prioridade\":\"Media\",\"justificativa\":\"Teste\",\"sugestaoResposta\":\"Teste\",\"extra\":true}")]
    [InlineData("{\"categoria\":\"Entrega\",\"categoria\":\"Outro\",\"prioridade\":\"Media\",\"justificativa\":\"Teste\",\"sugestaoResposta\":\"Teste\"}")]
    public void Malformed_or_incomplete_output_is_rejected(string json) => AssertInvalid(json);

    [Theory]
    [InlineData(0, 10)]
    [InlineData(401, 10)]
    [InlineData(10, 0)]
    [InlineData(10, 1201)]
    public void Generated_text_is_bounded(int reasonLength, int responseLength) =>
        AssertInvalid(JsonSerializer.Serialize(new
        {
            categoria = "Outro", prioridade = "Baixa",
            justificativa = new string('a', reasonLength),
            sugestaoResposta = new string('b', responseLength)
        }));

    [Fact]
    public void Whitespace_text_is_rejected() => AssertInvalid(JsonSerializer.Serialize(new
    {
        categoria = "Outro", prioridade = "Baixa", justificativa = "   ", sugestaoResposta = "Teste fictício."
    }));

    [Fact]
    public void Evaluation_dataset_has_fifteen_unique_valid_cases()
    {
        using var data = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "dataset.json")));
        var rows = data.RootElement.EnumerateArray().ToArray();
        Assert.Equal(15, rows.Length);
        Assert.Equal(15, rows.Select(row => row.GetProperty("id").GetString()).Distinct().Count());
        foreach (var row in rows)
        {
            Assert.Null(TicketRules.ValidateMessage(row.GetProperty("message").GetString()));
            Assert.Contains(row.GetProperty("categoria").GetString(), TicketRules.Categories);
            Assert.Contains(row.GetProperty("prioridade").GetString(), TicketRules.Priorities);
        }
    }

    internal static string Output(string category = "Entrega", string priority = "Media") =>
        JsonSerializer.Serialize(new
        {
            categoria = category, prioridade = priority,
            justificativa = "Solicitação fictícia.", sugestaoResposta = "Podemos verificar o pedido."
        });

    private static void AssertInvalid(string json)
    {
        var error = Assert.Throws<AnalysisException>(() => TicketOutputParser.Parse(json));
        Assert.Equal("invalid_provider_response", error.Code);
        Assert.Equal(502, error.StatusCode);
    }
}
