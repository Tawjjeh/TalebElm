using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TalebElm.Api.Middlewares;
using TalebElm.Domain.Exceptions;

namespace TalebElm.Tests.IntegrationTests;

public class CorrelationIdIntegrationTests
{
    private sealed record LogRecord(LogLevel Level, string Message);

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<LogRecord> Records { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Records.Add(new LogRecord(logLevel, formatter(state, exception)));
        }
    }

    private static async Task<(HttpContext Context, string Body)> ExecutePipelineAsync(
        string? incomingCorrelationId,
        RequestDelegate terminalDelegate,
        CapturingLogger<ExceptionHandlingMiddleware>? exceptionLogger = null)
    {
        var services = new ServiceCollection();
        if (exceptionLogger != null)
        {
            services.AddSingleton<ILogger<ExceptionHandlingMiddleware>>(exceptionLogger);
        }

        var exceptionMiddleware = new ExceptionHandlingMiddleware(terminalDelegate);
        var correlationMiddleware = new CorrelationIdMiddleware(ctx => exceptionMiddleware.InvokeAsync(ctx));

        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
        context.Request.Method = "GET";
        context.Request.Path = "/api/test";
        context.Response.Body = new MemoryStream();

        if (incomingCorrelationId is not null)
        {
            context.Request.Headers[CorrelationIdMiddleware.HeaderName] = incomingCorrelationId;
        }

        await correlationMiddleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();

        return (context, body);
    }

    [Fact]
    public async Task Pipeline_ValidCorrelationId_EchoesInResponseHeader()
    {
        const string validCorrelationId = "test-corr-abc-123";

        var (context, _) = await ExecutePipelineAsync(
            incomingCorrelationId: validCorrelationId,
            terminalDelegate: ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status200OK;
                return Task.CompletedTask;
            });

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(validCorrelationId, context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
        Assert.Equal(validCorrelationId, context.TraceIdentifier);
    }

    [Fact]
    public async Task Pipeline_MissingCorrelationId_FallsBackToTraceIdentifier()
    {
        var (context, _) = await ExecutePipelineAsync(
            incomingCorrelationId: null,
            terminalDelegate: ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status200OK;
                return Task.CompletedTask;
            });

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        var responseHeader = context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString();
        Assert.False(string.IsNullOrWhiteSpace(responseHeader));
        Assert.Equal(context.TraceIdentifier, responseHeader);
    }

    [Fact]
    public async Task Pipeline_OversizedCorrelationId_FallsBackToTraceIdentifier()
    {
        var oversized = new string('x', CorrelationIdMiddleware.MaxCorrelationIdLength + 20);

        var (context, _) = await ExecutePipelineAsync(
            incomingCorrelationId: oversized,
            terminalDelegate: ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status200OK;
                return Task.CompletedTask;
            });

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        var responseHeader = context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString();
        Assert.NotEqual(oversized, responseHeader);
        Assert.Equal(context.TraceIdentifier, responseHeader);
    }

    [Fact]
    public async Task Pipeline_InvalidCorrelationId_FallsBackToTraceIdentifier()
    {
        const string invalid = "bad id with spaces\r\n";

        var (context, _) = await ExecutePipelineAsync(
            incomingCorrelationId: invalid,
            terminalDelegate: ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status200OK;
                return Task.CompletedTask;
            });

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        var responseHeader = context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString();
        Assert.NotEqual(invalid, responseHeader);
        Assert.Equal(context.TraceIdentifier, responseHeader);
    }

    [Fact]
    public async Task Pipeline_UnhandledException_Returns500WithCorrelationIdInHeaderAndProblemDetails()
    {
        const string validCorrelationId = "unhandled-corr-999";
        var logger = new CapturingLogger<ExceptionHandlingMiddleware>();

        var (context, body) = await ExecutePipelineAsync(
            incomingCorrelationId: validCorrelationId,
            terminalDelegate: _ => throw new InvalidOperationException("Simulated unexpected failure."),
            exceptionLogger: logger);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal(validCorrelationId, context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());

        using var doc = JsonDocument.Parse(body);
        var traceId = doc.RootElement.GetProperty("traceId").GetString();
        Assert.Equal(validCorrelationId, traceId);

        var errorLog = logger.Records.FirstOrDefault(r => r.Level == LogLevel.Error);
        Assert.NotNull(errorLog);
        Assert.Contains(validCorrelationId, errorLog!.Message);
        Assert.DoesNotContain("Simulated unexpected failure.", errorLog.Message);
    }

    [Fact]
    public async Task Pipeline_DomainNotFound_Returns404WithCorrelationIdInHeaderAndProblemDetails()
    {
        const string validCorrelationId = "not-found-corr-404";
        var logger = new CapturingLogger<ExceptionHandlingMiddleware>();

        var (context, body) = await ExecutePipelineAsync(
            incomingCorrelationId: validCorrelationId,
            terminalDelegate: _ => throw new NotFoundException("Entity missing."),
            exceptionLogger: logger);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal(validCorrelationId, context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());

        using var doc = JsonDocument.Parse(body);
        var traceId = doc.RootElement.GetProperty("traceId").GetString();
        Assert.Equal(validCorrelationId, traceId);
    }
}

