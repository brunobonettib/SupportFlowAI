using SupportFlowAI.Evaluation;
using SupportFlowAI.OpenAI;
using SupportFlowAI.Ollama;
using Microsoft.Extensions.Options;
using SupportFlowAI.Tickets;

var evaluate = args.Contains("--evaluate");
var allowPaidEvaluation = args.Contains("--allow-paid-evaluation");
var allowLocalEvaluation = args.Contains("--allow-local-evaluation");
var warmUp = args.Contains("--warm-up");
var evaluationSet = args.FirstOrDefault(arg => arg.StartsWith("--evaluation-set=", StringComparison.Ordinal))?.Split('=', 2)[1] ?? "original";
var hostArgs = args.Where(arg => arg is not "--evaluate" and not "--allow-paid-evaluation" and not "--allow-local-evaluation" and not "--warm-up"
    && !arg.StartsWith("--evaluation-set=", StringComparison.Ordinal)).ToArray();
var builder = WebApplication.CreateBuilder(hostArgs);
// Console apenas: sem escrita no Event Log ou em arquivos do sistema.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.WebHost.ConfigureKestrel(server => server.Limits.MaxRequestBodySize = 32768);
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow;
    options.SerializerOptions.AllowDuplicateProperties = false;
});
builder.Services.AddOptions<AnalysisOptions>().Bind(builder.Configuration.GetSection("Analysis"))
    .Validate(options => options.IsValid(), "Analysis:Provider deve ser Ollama ou OpenAI.").ValidateOnStart();
builder.Services.AddOptions<OllamaOptions>().Bind(builder.Configuration.GetSection("Ollama"))
    .Validate(options => options.IsValid(), "Configuração Ollama inválida.");
builder.Services.AddOptions<OpenAiOptions>()
    .Bind(builder.Configuration.GetSection("OpenAI"))
    .PostConfigure(options =>
    {
        if (!options.HasApiKey)
            options.ApiKey = builder.Configuration["OPENAI_API_KEY"] ?? "";
    })
    .Validate(options => options.IsValid(), "Configuração OpenAI inválida: confira modelo, esforço, timeout e limite de tokens.");
builder.Services.AddHttpClient<OpenAiTicketAnalyzer>(client =>
    client.Timeout = Timeout.InfiniteTimeSpan)
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false })
    .RemoveAllLoggers();
builder.Services.AddHttpClient<OllamaTicketAnalyzer>(client => client.Timeout = Timeout.InfiniteTimeSpan)
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false })
    .RemoveAllLoggers();
builder.Services.AddTransient<ITicketAnalyzer>(services =>
    services.GetRequiredService<IOptions<AnalysisOptions>>().Value.Provider == "Ollama"
        ? services.GetRequiredService<OllamaTicketAnalyzer>()
        : services.GetRequiredService<OpenAiTicketAnalyzer>());

var app = builder.Build();

if (evaluate)
{
    using var cancellation = new CancellationTokenSource();
    ConsoleCancelEventHandler onCancel = (_, e) => { e.Cancel = true; cancellation.Cancel(); };
    Console.CancelKeyPress += onCancel;
    try
    {
        Environment.ExitCode = await EvaluationRunner.RunAsync(app.Services, app.Environment.ContentRootPath,
            allowPaidEvaluation, allowLocalEvaluation, cancellation.Token, evaluationSet, warmUp);
    }
    finally { Console.CancelKeyPress -= onCancel; }
    await app.DisposeAsync();
    return;
}

// Diagnóstico controlado: não registrar corpo, headers ou mensagens de exceções.
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    SuppressDiagnosticsCallback = _ => true,
    ExceptionHandler = async context =>
    {
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        var status = error is BadHttpRequestException badRequest ? badRequest.StatusCode : 500;
        app.Logger.LogWarning("Falha HTTP: status={Status}; traceId={TraceId}", status, context.TraceIdentifier);
        await Results.Problem(statusCode: status,
            title: status < 500 ? "Requisição inválida." : "Erro interno ao processar a solicitação.",
            extensions: new Dictionary<string, object?> { ["traceId"] = context.TraceIdentifier }).ExecuteAsync(context);
    }
});
app.UseStatusCodePages();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    // API fora de Development rejeita HTTP; não redireciona corpos potencialmente sensíveis.
    app.Use(async (context, next) =>
    {
        if (!context.Request.IsHttps)
        {
            await Results.Problem(statusCode: 400, title: "HTTPS obrigatório.").ExecuteAsync(context);
            return;
        }
        await next(context);
    });
}

app.MapPost("/api/tickets/analyze", async (AnalyzeTicketRequest? request, ITicketAnalyzer analyzer,
    HttpContext context, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    var error = TicketRules.ValidateMessage(request?.Message);
    if (error is not null)
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["message"] = [error] });
    try
    {
        return Results.Ok(await analyzer.AnalyzeAsync(request!.Message!.Trim(), cancellationToken));
    }
    catch (AnalysisException exception)
    {
        logger.LogWarning("Falha na análise: {Code}; traceId={TraceId}", exception.Code, context.TraceIdentifier);
        return Results.Problem(statusCode: exception.StatusCode, title: "Não foi possível analisar a mensagem.",
            detail: exception.Message, extensions: new Dictionary<string, object?>
            {
                ["code"] = exception.Code,
                ["traceId"] = context.TraceIdentifier
            });
    }
})
.WithName("AnalyzeTicket")
.WithSummary("Classifica um ticket e sugere resposta para revisão humana.")
.Produces<TicketAnalysis>()
.ProducesValidationProblem()
.ProducesProblem(502)
.ProducesProblem(503)
.ProducesProblem(504);

app.Run();

public partial class Program;
