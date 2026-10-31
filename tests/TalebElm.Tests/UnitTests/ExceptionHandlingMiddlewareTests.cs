using System.Net;
using System.Text.Json;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using TalebElm.Api.Middlewares;
using DomainValidationException = TalebElm.Domain.Exceptions.ValidationException;
using FluentValidationException = FluentValidation.ValidationException;

namespace TalebElm.Tests.UnitTests;

public class ExceptionHandlingMiddlewareTests
{
    private static async Task<(int StatusCode, string ContentType, JsonDocument Document)> InvokeMiddlewareWithAsync(
        Exception exception)
    {
        var middleware = new ExceptionHandlingMiddleware(_ => throw exception);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var content = await reader.ReadToEndAsync();
        var document = JsonDocument.Parse(content);

        return (context.Response.StatusCode, context.Response.ContentType ?? string.Empty, document);
    }

    [Fact]
    public async Task FluentValidation_SingleFailureOnSingleField_ReturnsFormattedFieldMap()
    {
        var failures = new[]
        {
            new ValidationFailure("Name", "'Name' must not be empty.")
        };
        var exception = new FluentValidationException(failures);

        var (statusCode, contentType, doc) = await InvokeMiddlewareWithAsync(exception);

        Assert.Equal((int)HttpStatusCode.BadRequest, statusCode);
        Assert.Equal("application/problem+json", contentType);

        var root = doc.RootElement;
        Assert.Equal(400, root.GetProperty("status").GetInt32());
        Assert.Equal("One or more validation errors occurred.", root.GetProperty("title").GetString());
        Assert.True(root.TryGetProperty("errors", out var errors));

        var nameErrors = errors.GetProperty("name").EnumerateArray().Select(e => e.GetString()).ToArray();
        Assert.Single(nameErrors);
        Assert.Equal("'Name' must not be empty.", nameErrors[0]);
    }

    [Fact]
    public async Task FluentValidation_MultipleFailuresOnSingleField_ReturnsAllMessagesInArray()
    {
        var failures = new[]
        {
            new ValidationFailure("Password", "'Password' must be at least 8 characters."),
            new ValidationFailure("Password", "'Password' must contain at least one digit.")
        };
        var exception = new FluentValidationException(failures);

        var (statusCode, _, doc) = await InvokeMiddlewareWithAsync(exception);

        Assert.Equal(400, statusCode);
        var errors = doc.RootElement.GetProperty("errors");
        var passwordErrors = errors.GetProperty("password").EnumerateArray().Select(e => e.GetString()).ToArray();
        Assert.Equal(2, passwordErrors.Length);
        Assert.Contains("'Password' must be at least 8 characters.", passwordErrors);
        Assert.Contains("'Password' must contain at least one digit.", passwordErrors);
    }

    [Fact]
    public async Task FluentValidation_FailuresAcrossMultipleFields_ReturnsAllKeysInCamelCase()
    {
        var failures = new[]
        {
            new ValidationFailure("TrackTitle", "'TrackTitle' is required."),
            new ValidationFailure("PassThreshold", "'PassThreshold' must be between 1 and 100.")
        };
        var exception = new FluentValidationException(failures);

        var (statusCode, _, doc) = await InvokeMiddlewareWithAsync(exception);

        Assert.Equal(400, statusCode);
        var errors = doc.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("trackTitle", out _));
        Assert.True(errors.TryGetProperty("passThreshold", out _));
    }

    [Fact]
    public async Task FluentValidation_EmptyPropertyName_FallsBackToTopLevelDetail()
    {
        var failures = new[]
        {
            new ValidationFailure("", "General payload validation failure.")
        };
        var exception = new FluentValidationException(failures);

        var (statusCode, _, doc) = await InvokeMiddlewareWithAsync(exception);

        Assert.Equal(400, statusCode);
        var root = doc.RootElement;
        Assert.Equal("General payload validation failure.", root.GetProperty("detail").GetString());
        Assert.False(root.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task DomainValidationException_DoesNotEmitFieldErrorsMap()
    {
        var exception = new DomainValidationException("A core domain rule failed.");

        var (statusCode, _, doc) = await InvokeMiddlewareWithAsync(exception);

        Assert.Equal(400, statusCode);
        var root = doc.RootElement;
        Assert.Equal("A business rule was violated.", root.GetProperty("title").GetString());
        Assert.Equal("A core domain rule failed.", root.GetProperty("detail").GetString());
        Assert.False(root.TryGetProperty("errors", out _));
    }
}
