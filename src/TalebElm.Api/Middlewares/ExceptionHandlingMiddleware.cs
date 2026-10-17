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

        return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }

    private static (HttpStatusCode StatusCode, string Title) Map(Exception exception)
        => exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
            TalebElm.Domain.Exceptions.ValidationException => (HttpStatusCode.BadRequest, "A business rule was violated."),
            FluentValidation.ValidationException => (HttpStatusCode.BadRequest, "One or more validation errors occurred."),
            TalebElm.Domain.Exceptions.NotImplementedException => (HttpStatusCode.NotImplemented, "The requested capability is not implemented yet."),
            DomainException => (HttpStatusCode.BadRequest, "The request could not be processed."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };
}
