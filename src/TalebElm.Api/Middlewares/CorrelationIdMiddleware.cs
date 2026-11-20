using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TalebElm.Api.Middlewares;

public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    public const int MaxCorrelationIdLength = 64;

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware>? _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware>? logger = null)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var rawHeader = context.Request.Headers[HeaderName].ToString();

        var correlationId = IsValidCorrelationId(rawHeader)
            ? rawHeader
            : context.TraceIdentifier;

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.TraceIdentifier = correlationId;

        if (!context.Response.HasStarted)
        {
            context.Response.Headers[HeaderName] = correlationId;
        }

        context.Items[HeaderName] = correlationId;

        var logger = _logger ?? context.RequestServices?.GetService<ILogger<CorrelationIdMiddleware>>();
        if (logger is not null)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            }))
            {
                await _next(context);
            }
        }
        else
        {
            await _next(context);
        }
    }

    public static bool IsValidCorrelationId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > MaxCorrelationIdLength)
        {
            return false;
        }

        foreach (var ch in value)
        {
            if (!IsValidChar(ch))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidChar(char c) =>
        (c >= 'a' && c <= 'z') ||
        (c >= 'A' && c <= 'Z') ||
        (c >= '0' && c <= '9') ||
        c is '-' or '_' or '.' or ':';
}

