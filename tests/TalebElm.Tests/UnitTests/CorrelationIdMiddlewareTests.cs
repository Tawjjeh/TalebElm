using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TalebElm.Api.Middlewares;

namespace TalebElm.Tests.UnitTests;

public class CorrelationIdMiddlewareTests
{
    private sealed class ScopeCapturingLogger<T> : ILogger<T>
    {
        public List<object?> CapturedScopes { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            CapturedScopes.Add(state);
            return new DummyDisposable();
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }

        private sealed class DummyDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }

    [Theory]
    [InlineData("valid-correlation-id")]
    [InlineData("c56a4180-65aa-42ec-a945-5fd21dec0538")]
    [InlineData("req_123.456:789")]
    [InlineData("Trace-1234567890_abcdef")]
    public void IsValidCorrelationId_WhenValid_ReturnsTrue(string validId)
    {
        Assert.True(CorrelationIdMiddleware.IsValidCorrelationId(validId));
    }

    [Fact]
    public void IsValidCorrelationId_WhenMaxLength_ReturnsTrue()
    {
        var exactMaxLength = new string('a', CorrelationIdMiddleware.MaxCorrelationIdLength);
        Assert.True(CorrelationIdMiddleware.IsValidCorrelationId(exactMaxLength));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void IsValidCorrelationId_WhenNullOrWhiteSpace_ReturnsFalse(string? emptyId)
    {
        Assert.False(CorrelationIdMiddleware.IsValidCorrelationId(emptyId));
    }

    [Fact]
    public void IsValidCorrelationId_WhenOversized_ReturnsFalse()
    {
        var oversizedId = new string('a', CorrelationIdMiddleware.MaxCorrelationIdLength + 1);
        Assert.False(CorrelationIdMiddleware.IsValidCorrelationId(oversizedId));
    }

    [Theory]
    [InlineData("invalid id with spaces")]
    [InlineData("invalid\r\nnewlines")]
    [InlineData("invalid\tTab")]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("invalid@char#")]
    [InlineData("invalid;semi")]
    [InlineData("invalid'quote")]
    public void IsValidCorrelationId_WhenContainsInvalidCharacters_ReturnsFalse(string invalidId)
    {
        Assert.False(CorrelationIdMiddleware.IsValidCorrelationId(invalidId));
    }

    [Fact]
    public async Task InvokeAsync_WhenValidHeaderProvided_SetsTraceIdentifierAndResponseHeader()
    {
        const string incomingId = "client-trace-12345";
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = incomingId;

        var invokedNext = false;
        var middleware = new CorrelationIdMiddleware(ctx =>
        {
            invokedNext = true;
            Assert.Equal(incomingId, ctx.TraceIdentifier);
            Assert.Equal(incomingId, ctx.Items[CorrelationIdMiddleware.HeaderName]);
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(invokedNext);
        Assert.Equal(incomingId, context.TraceIdentifier);
        Assert.Equal(incomingId, context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
    }

    [Fact]
    public async Task InvokeAsync_WhenHeaderMissing_FallsBackToTraceIdentifier()
    {
        var context = new DefaultHttpContext();
        var originalTraceId = context.TraceIdentifier;

        var invokedNext = false;
        var middleware = new CorrelationIdMiddleware(ctx =>
        {
            invokedNext = true;
            Assert.Equal(originalTraceId, ctx.TraceIdentifier);
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(invokedNext);
        Assert.Equal(originalTraceId, context.TraceIdentifier);
        Assert.Equal(originalTraceId, context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
    }

    [Fact]
    public async Task InvokeAsync_WhenHeaderOversized_FallsBackToTraceIdentifier()
    {
        var context = new DefaultHttpContext();
        var originalTraceId = context.TraceIdentifier;
        var oversizedHeader = new string('z', CorrelationIdMiddleware.MaxCorrelationIdLength + 10);
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = oversizedHeader;

        var invokedNext = false;
        var middleware = new CorrelationIdMiddleware(ctx =>
        {
            invokedNext = true;
            Assert.Equal(originalTraceId, ctx.TraceIdentifier);
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(invokedNext);
        Assert.Equal(originalTraceId, context.TraceIdentifier);
        Assert.Equal(originalTraceId, context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
    }

    [Fact]
    public async Task InvokeAsync_WhenHeaderInvalidFormat_FallsBackToTraceIdentifier()
    {
        var context = new DefaultHttpContext();
        var originalTraceId = context.TraceIdentifier;
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "invalid id with spaces";

        var invokedNext = false;
        var middleware = new CorrelationIdMiddleware(ctx =>
        {
            invokedNext = true;
            Assert.Equal(originalTraceId, ctx.TraceIdentifier);
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        Assert.True(invokedNext);
        Assert.Equal(originalTraceId, context.TraceIdentifier);
        Assert.Equal(originalTraceId, context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
    }

    [Fact]
    public async Task InvokeAsync_CreatesLogScopeWithCorrelationId()
    {
        const string correlationId = "scope-test-corr-id";
        var logger = new ScopeCapturingLogger<CorrelationIdMiddleware>();

        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = correlationId;

        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask, logger);

        await middleware.InvokeAsync(context);

        Assert.Single(logger.CapturedScopes);
        var scopeDict = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(logger.CapturedScopes[0]);
        Assert.True(scopeDict.TryGetValue("CorrelationId", out var capturedVal));
        Assert.Equal(correlationId, capturedVal);
    }
}

