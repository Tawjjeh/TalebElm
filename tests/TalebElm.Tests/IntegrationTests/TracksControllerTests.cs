using Microsoft.AspNetCore.Http;
using TalebElm.Api.Controllers;
using TalebElm.Api.Middlewares;
using DomainNotImplemented = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Tests.IntegrationTests;

public class TracksControllerTests
{
    [Fact]
    public void Get_ThrowsDomainNotImplementedException()
    {
        var exception = Assert.Throws<DomainNotImplemented>(() => new TracksController().Get());

        Assert.Contains("not implemented", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Middleware_MapsDomainNotImplementedExceptionTo501Problem()
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new DomainNotImplemented("Listing tracks is not implemented yet."));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status501NotImplemented, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("\"status\":501", body);
    }
}
