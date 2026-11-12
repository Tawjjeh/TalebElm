using System.Text.Json;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using TalebElm.Api.Middlewares;
using TalebElm.Domain.Exceptions;
using DomainValidationException = TalebElm.Domain.Exceptions.ValidationException;
using FluentValidationException = FluentValidation.ValidationException;

namespace TalebElm.Tests.IntegrationTests;

public class ExceptionHandlingMiddlewareTests
{
    private static async Task<(int StatusCode, string ContentType, string Body)> InvokeAsync(Exception exception)
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw exception);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();

        return (context.Response.StatusCode, context.Response.ContentType ?? string.Empty, body);
    }

    [Fact]
    public async Task InvokeAsync_WhenNotFoundExceptionThrown_MapsTo404Problem()
    {
        var (statusCode, contentType, body) = await InvokeAsync(new NotFoundException("Track not found."));

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);
        Assert.Equal("application/problem+json", contentType);
        Assert.Contains("\"status\":404", body);
    }

    [Fact]
    public async Task InvokeAsync_WhenConflictExceptionThrown_MapsTo409Problem()
    {
        var (statusCode, contentType, body) = await InvokeAsync(new ConflictException("Duplicate record."));

        Assert.Equal(StatusCodes.Status409Conflict, statusCode);
        Assert.Equal("application/problem+json", contentType);
        Assert.Contains("\"status\":409", body);
    }

    [Fact]
    public async Task InvokeAsync_WhenDomainValidationExceptionThrown_MapsTo400Problem()
    {
        var (statusCode, contentType, body) = await InvokeAsync(new DomainValidationException("Invalid rule."));

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.Equal("application/problem+json", contentType);
        Assert.Contains("\"status\":400", body);
    }

    [Fact]
    public async Task InvokeAsync_WhenFluentValidationExceptionThrown_MapsTo400Problem()
    {
        var exception = new FluentValidationException([new ValidationFailure("Title", "Title is required.")]);

        var (statusCode, contentType, body) = await InvokeAsync(exception);

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.Equal("application/problem+json", contentType);
        Assert.Contains("\"status\":400", body);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledExceptionThrown_MapsTo500WithoutLeakingMessage()
    {
        const string secretInternalMessage = "Sensitive database connection failure details";

        var (statusCode, contentType, body) = await InvokeAsync(new InvalidOperationException(secretInternalMessage));

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
        Assert.Equal("application/problem+json", contentType);

        using var doc = JsonDocument.Parse(body);
        Assert.Equal(500, doc.RootElement.GetProperty("status").GetInt32());
        Assert.DoesNotContain(secretInternalMessage, body);
    }
}
