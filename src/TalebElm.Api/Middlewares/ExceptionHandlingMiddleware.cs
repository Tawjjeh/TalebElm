using System.Net;
using System.Text.Json;
using TalebElm.Domain.Exceptions;

namespace TalebElm.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await WriteProblemAsync(context, ex);
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        if (exception is FluentValidation.ValidationException validationException)
        {
            await WriteValidationProblemAsync(context, validationException);
            return;
        }
        var (statusCode, title) = Map(exception);
        context.Response.StatusCode = (int)statusCode;

        // Never leak internal details for unhandled failures.
        var detail = statusCode == HttpStatusCode.InternalServerError
            ? title
            : exception.Message;

        var problem = new
        {
            status = (int)statusCode,
            title,
            detail
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }

    private static (HttpStatusCode StatusCode, string Title) Map(Exception exception)
        => exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
            TalebElm.Domain.Exceptions.ValidationException => (HttpStatusCode.BadRequest, "A business rule was violated."),
            ConflictException => (HttpStatusCode.Conflict, "A conflicting state or duplicate resource was detected."),
            TalebElm.Domain.Exceptions.NotImplementedException => (HttpStatusCode.NotImplemented, "The requested capability is not implemented yet."),
            DomainException => (HttpStatusCode.BadRequest, "The request could not be processed."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

    private static async Task WriteValidationProblemAsync(HttpContext context, FluentValidation.ValidationException validationException)
    {
        var failures = validationException.Errors.ToList();
        var errors = failures.Where(f => !string.IsNullOrWhiteSpace(f.PropertyName))
            .GroupBy(f => JsonNamingPolicy.CamelCase.ConvertName(f.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

        var nonFieldMessages = failures.Where(f => string.IsNullOrWhiteSpace(f.PropertyName))
            .Select(f => f.ErrorMessage).Distinct().ToArray();

        var detail = nonFieldMessages.Length > 0
            ? string.Join(" ", nonFieldMessages)
            : "One or more validation errors occurred.";

        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        var problem = new
        {
            status = (int)HttpStatusCode.BadRequest,
            title = "One or more validation errors occurred.",
            detail,
            errors = errors.Count > 0 ? errors : null
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}




