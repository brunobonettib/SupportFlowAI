using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SupportFlowAI.Tickets;
using Xunit;

namespace SupportFlowAI.Tests;

public class TicketEndpointTests
{
    [Theory]
    [InlineData("{\"message\":\"\"}")]
    [InlineData("{\"message\":\"   \"}")]
    [InlineData("{\"message\":null}")]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("{")]
    [InlineData("{\"message\":123}")]
    [InlineData("{\"message\":\"Primeira\",\"message\":\"Segunda\"}")]
    [InlineData("{\"message\":\"Teste\",\"private-field\":true}")]
    public async Task Invalid_requests_return_400_without_calling_analyzer(string body)
    {
        var analyzer = new StubAnalyzer();
        await using var factory = CreateFactory(analyzer);
        using var client = factory.CreateClient();
        using var response = await client.PostAsync("/api/tickets/analyze", new StringContent(body, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, analyzer.Calls);
        var error = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("System.Text.Json", error);
        Assert.DoesNotContain("private-field", error);
    }

    [Fact]
    public async Task Oversized_message_returns_400_without_calling_analyzer()
    {
        var analyzer = new StubAnalyzer();
        await using var factory = CreateFactory(analyzer);
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/tickets/analyze", new { message = new string('a', 4001) });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, analyzer.Calls);
    }

    [Fact]
    public async Task Wrong_content_type_returns_415()
    {
        var analyzer = new StubAnalyzer();
        await using var factory = CreateFactory(analyzer);
        using var client = factory.CreateClient();
        using var response = await client.PostAsync("/api/tickets/analyze", new StringContent("Teste fictício"));
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        Assert.Equal(0, analyzer.Calls);
    }

    [Fact]
    public async Task Valid_request_returns_review_flag_and_json_contract()
    {
        var analyzer = new StubAnalyzer();
        await using var factory = CreateFactory(analyzer);
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/tickets/analyze", new { message = "  Meu pedido não chegou.  " });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var output = await response.Content.ReadFromJsonAsync<TicketAnalysis>();
        Assert.NotNull(output);
        Assert.Equal("Entrega", output.Categoria);
        Assert.Equal("Media", output.Prioridade);
        Assert.Contains("\"requerRevisaoHumana\":true", await response.Content.ReadAsStringAsync());
        Assert.Equal("Meu pedido não chegou.", analyzer.Message);
        Assert.Equal(1, analyzer.Calls);
    }

    [Theory]
    [InlineData(502)]
    [InlineData(503)]
    [InlineData(504)]
    public async Task Provider_failures_use_problem_details(int status)
    {
        await using var factory = CreateFactory(new StubAnalyzer(new AnalysisException("test_error", status, "Erro fictício.")));
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/tickets/analyze", new { message = "Teste fictício" });
        Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains("\"code\":\"test_error\"", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Unexpected_exceptions_are_sanitized()
    {
        await using var factory = CreateFactory(new StubAnalyzer(new InvalidOperationException("private-exception-detail")));
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/tickets/analyze", new { message = "Teste fictício" });
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("private-exception-detail", body);
        Assert.DoesNotContain("stackTrace", body);
    }

    [Fact]
    public async Task Openapi_document_works_with_patched_dependency()
    {
        await using var factory = CreateFactory(new StubAnalyzer());
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/openapi/v1.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("/api/tickets/analyze", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Production_rejects_http_without_redirect_or_analysis()
    {
        var analyzer = new StubAnalyzer();
        await using var factory = CreateFactory(analyzer, "Production");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var response = await client.PostAsJsonAsync("/api/tickets/analyze", new { message = "Teste fictício" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(response.Headers.Location);
        Assert.Equal(0, analyzer.Calls);
    }

    private static WebApplicationFactory<Program> CreateFactory(StubAnalyzer analyzer, string environment = "Development") =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAI:ApiKey"] = "", ["OPENAI_API_KEY"] = "", ["OpenAI:Model"] = "test-model",
                ["OpenAI:ReasoningEffort"] = "none", ["OpenAI:TimeoutSeconds"] = "30", ["OpenAI:MaxOutputTokens"] = "1000"
            }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITicketAnalyzer>();
                services.AddSingleton<ITicketAnalyzer>(analyzer);
            });
        });

    private sealed class StubAnalyzer(Exception? error = null) : ITicketAnalyzer
    {
        public int Calls { get; private set; }
        public string? Message { get; private set; }
        public Task<TicketAnalysis> AnalyzeAsync(string message, CancellationToken cancellationToken)
        {
            Calls++;
            Message = message;
            if (error is not null) throw error;
            return Task.FromResult(new TicketAnalysis("Entrega", "Media", "Justificativa fictícia.", "Sugestão fictícia."));
        }
    }
}
