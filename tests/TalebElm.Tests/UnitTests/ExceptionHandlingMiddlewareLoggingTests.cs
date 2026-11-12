using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TalebElm.Api.Middlewares;
using TalebElm.Domain.Exceptions;

namespace TalebElm.Tests.UnitTests;

public class ExceptionHandlingMiddlewareLoggingTests
{
    private sealed record LogEntry(LogLevel Level, string Message);

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(logLevel, formatter(state, exception)));
        }
    }

    [Fact]
    public async Task UnhandledException_Returns500_AndLogsErrorWithRequestPath()
    {
        var logger = new CapturingLogger<ExceptionHandlingMiddleware>();
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<ExceptionHandlingMiddleware>>(logger);

        var middleware = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException("boom"));
        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
        context.Request.Method = "GET";
        context.Request.Path = "/api/tracks/42";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(500, context.Response.StatusCode);
        var entry = logger.Entries.FirstOrDefault(e => e.Level == LogLevel.Error);
        Assert.True(entry is not null, "Expected an error log entry for the unhandled 500 but none was written.");
        Assert.Contains("/api/tracks/42", entry!.Message);
    }

    [Fact]
    public async Task DomainNotFound_Returns404_WithoutErrorLogAndReturnsTraceId()
    {
        var logger = new CapturingLogger<ExceptionHandlingMiddleware>();
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<ExceptionHandlingMiddleware>>(logger);

        var middleware = new ExceptionHandlingMiddleware(_ => throw new NotFoundException("Track not found."));
        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
        context.Request.Method = "GET";
        context.Request.Path = "/api/tracks/42";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(404, context.Response.StatusCode);
        Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Error);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);
        var traceId = doc.RootElement.GetProperty("traceId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(traceId), "Expected a non-empty traceId in the problem response.");
    }

    [Fact]
    public async Task UnhandledException_ResponseIncludesTraceId_AndLogDoesNotContainExceptionMessage()
    {
        var logger = new CapturingLogger<ExceptionHandlingMiddleware>();
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<ExceptionHandlingMiddleware>>(logger);

        const string sensitive = "SELECT * FROM Users WHERE Password = 'x'";
        var middleware = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException(sensitive));
        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
        context.Request.Method = "GET";
        context.Request.Path = "/api/tracks/42";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(500, context.Response.StatusCode);
        Assert.DoesNotContain(logger.Entries, e => e.Message.Contains(sensitive));

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.False(string.IsNullOrWhiteSpace(doc.RootElement.GetProperty("traceId").GetString()));
    }
}
