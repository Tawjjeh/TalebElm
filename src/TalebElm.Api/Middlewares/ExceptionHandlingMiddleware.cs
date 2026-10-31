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

    private static Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = Map(exception);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        if (exception is FluentValidation.ValidationException fvEx)
        {
            var fieldErrors = fvEx.Errors
                .Where(e => !string.IsNullOrWhiteSpace(e.PropertyName))
                .GroupBy(e => ToCamelCase(e.PropertyName))
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            var topLevelMessages = fvEx.Errors
                .Where(e => string.IsNullOrWhiteSpace(e.PropertyName))
                .Select(e => e.ErrorMessage)
                .ToList();

            var detail = topLevelMessages.Count > 0
                ? string.Join(" ", topLevelMessages)
                : title;

            var problem = new Dictionary<string, object?>
            {
                ["status"] = (int)statusCode,
                ["title"] = title,
                ["detail"] = detail
            };

            if (fieldErrors.Count > 0)
            {
                problem["errors"] = fieldErrors;
            }

            return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }

        // Never leak internal details for unhandled failures.
        var standardDetail = statusCode == HttpStatusCode.InternalServerError
            ? title
            : exception.Message;

        var standardProblem = new
        {
            status = (int)statusCode,
            title,
            detail = standardDetail
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(standardProblem));
    }

    private static string ToCamelCase(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return propertyName;

        var parts = propertyName.Split('.');
        for (var i = 0; i < parts.Length; i++)
        {
            parts[i] = JsonNamingPolicy.CamelCase.ConvertName(parts[i]);
        }
        return string.Join('.', parts);
    }

    private static (HttpStatusCode StatusCode, string Title) Map(Exception exception)
        => exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
            TalebElm.Domain.Exceptions.ValidationException => (HttpStatusCode.BadRequest, "A business rule was violated."),
            ConflictException => (HttpStatusCode.Conflict, "A conflicting state or duplicate resource was detected."),
            FluentValidation.ValidationException => (HttpStatusCode.BadRequest, "One or more validation errors occurred."),
            TalebElm.Domain.Exceptions.NotImplementedException => (HttpStatusCode.NotImplemented, "The requested capability is not implemented yet."),
            DomainException => (HttpStatusCode.BadRequest, "The request could not be processed."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };
}
