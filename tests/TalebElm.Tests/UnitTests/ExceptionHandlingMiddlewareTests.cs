using System;
using System.Net;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using TalebElm.Api.Middlewares;
using Xunit;

namespace TalebElm.Tests.UnitTests;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_FluentValidationException_ReturnsSingleFieldError()
    {
        // Arrange
        var exception = new FluentValidation.ValidationException(
        [
            new ValidationFailure("Email", "Email is required.")
        ]);

        // Act
        var response = await ExecuteMiddlewareAsync(exception);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, response.Status);
        var errors = response.Body.RootElement.GetProperty("errors");
        Assert.Equal("Email is required.", errors.GetProperty("email")[0].GetString());
    }

    [Fact]
    public async Task InvokeAsync_MultipleFailuresOnSameField_GroupsMessages()
    {
        // Arrange
        var exception = new FluentValidation.ValidationException(
        [
            new ValidationFailure("Email", "Email is required."),
            new ValidationFailure("Email", "Email is invalid.")
        ]);

        // Act
        var response = await ExecuteMiddlewareAsync(exception);

        // Assert
        var messages = response.Body.RootElement.GetProperty("errors").GetProperty("email");

        Assert.Equal(2, messages.GetArrayLength());
        Assert.Equal("Email is required.", messages[0].GetString());
        Assert.Equal("Email is invalid.", messages[1].GetString());
    }

    [Fact]
    public async Task InvokeAsync_MultipleFields_ReturnsSeparateFieldErrors()
    {
        // Arrange
        var exception = new FluentValidation.ValidationException(
        [
            new ValidationFailure("Email", "Email is required."),
            new ValidationFailure("Password", "Password is too short."),
            new ValidationFailure("UserName", "Username is already taken.")
        ]);

        // Act
        var response = await ExecuteMiddlewareAsync(exception);

        // Assert
        var errors = response.Body.RootElement.GetProperty("errors");

        Assert.Equal("Email is required.", errors.GetProperty("email")[0].GetString());
        Assert.Equal("Password is too short.", errors.GetProperty("password")[0].GetString());
        Assert.Equal("Username is already taken.", errors.GetProperty("userName")[0].GetString());
        Assert.Equal(3, errors.EnumerateObject().Count());
    }

    [Fact]
    public async Task InvokeAsync_EmptyPropertyName_ReturnsTopLevelDetail()
    {
        // Arrange
        var exception = new FluentValidation.ValidationException([new ValidationFailure("", "A general validation error occurred.")]);

        // Act
        var response = await ExecuteMiddlewareAsync(exception);

        // Assert
        var body = response.Body.RootElement;

        Assert.Equal("A general validation error occurred.", body.GetProperty("detail").GetString());
         Assert.Equal(JsonValueKind.Null, body.GetProperty("errors").ValueKind);
    }

    private static async Task<(int Status, JsonDocument Body)> ExecuteMiddlewareAsync(Exception exception)
    {
        var context = new DefaultHttpContext();

        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => Task.FromException(exception));

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;

        var body = await JsonDocument.ParseAsync(context.Response.Body);

        return (context.Response.StatusCode, body);
    }



    [Fact]
    public async Task InvokeAsync_DomainValidationException_ReturnsSingleDetail()
    {
        // Arrange
        var exception = new TalebElm.Domain.Exceptions.ValidationException("A business rule was violated.");

        // Act
        var response = await ExecuteMiddlewareAsync(exception);

        // Assert
        var body = response.Body.RootElement;

        Assert.Equal(400, response.Status);
        Assert.Equal("A business rule was violated.", body.GetProperty("detail").GetString());
        Assert.False(body.TryGetProperty("errors", out _));
    }
}


