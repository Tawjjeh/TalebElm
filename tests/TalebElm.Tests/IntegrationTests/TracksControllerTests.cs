using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TalebElm.Api.Controllers;
using TalebElm.Api.Middlewares;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using DomainNotImplemented = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Tests.IntegrationTests;

public class TracksControllerTests
{
    private readonly Mock<ITrackService> _trackService = new();

    [Fact]
    public async Task Get_ReturnsOkWithPagedTracksResponse()
    {
        var expectedResponse = new PagedTracksResponse(
            [new TrackResponse(Guid.NewGuid(), "Backend .NET", "Description", 1)],
            Page: 1,
            PageSize: 20,
            TotalCount: 1);

        _trackService
            .Setup(s => s.GetPagedAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var controller = new TracksController(_trackService.Object);

        var actionResult = await controller.Get(1, 20);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var result = Assert.IsType<PagedTracksResponse>(okResult.Value);
        Assert.Equal(expectedResponse.Page, result.Page);
        Assert.Equal(expectedResponse.PageSize, result.PageSize);
        Assert.Equal(expectedResponse.TotalCount, result.TotalCount);
        Assert.Single(result.Items);
    }

    [Fact]
    public void Post_ThrowsDomainNotImplementedException()
    {
        var controller = new TracksController(_trackService.Object);

        var exception = Assert.Throws<DomainNotImplemented>(
            () => controller.Post(new CreateTrackRequest("Backend .NET", "Description")));

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
