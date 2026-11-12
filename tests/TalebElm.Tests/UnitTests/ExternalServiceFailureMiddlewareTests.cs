using Microsoft.AspNetCore.Http;
using TalebElm.Api.Middlewares;
using TalebElm.Domain.Exceptions;

namespace TalebElm.Tests.UnitTests;

public class ExternalServiceFailureMiddlewareTests
{
    [Fact]
    public async Task Middleware_MapsExternalServiceUnavailableTo503Problem()
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new ExternalServiceUnavailableException("GitHub is temporarily unavailable."));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("\"status\":503", body);
        Assert.Contains("external service", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Middleware_KeepsExternalFailureDistinctFromNotFound()
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new ExternalServiceUnavailableException("Codeforces timed out."));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        Assert.NotEqual(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }
}
