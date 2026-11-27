using Microsoft.AspNetCore.Authorization;
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
    public async Task Post_ReturnsCreatedWithTrackResponse()
    {
        var expected = new TrackResponse(Guid.NewGuid(), "Backend .NET", "Description", 0);

        _trackService
            .Setup(s => s.CreateAsync(It.IsAny<CreateTrackRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new TracksController(_trackService.Object);

        var actionResult = await controller.Post(new CreateTrackRequest("Backend .NET", "Description"));

        var createdResult = Assert.IsType<CreatedResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(expected, createdResult.Value);
    }

    [Fact]
    public async Task Put_ReturnsOkWithTrackResponse()
    {
        var trackId = Guid.NewGuid();
        var expected = new TrackResponse(trackId, "New", "New description", 0);

        _trackService
            .Setup(s => s.UpdateAsync(trackId, It.IsAny<UpdateTrackRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new TracksController(_trackService.Object);

        var actionResult = await controller.Put(trackId, new UpdateTrackRequest("New", "New description"));

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var trackId = Guid.NewGuid();
        var controller = new TracksController(_trackService.Object);

        var actionResult = await controller.Delete(trackId);

        Assert.IsType<NoContentResult>(actionResult);
        _trackService.Verify(s => s.DeleteAsync(trackId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Post")]
    [InlineData("Put")]
    [InlineData("Delete")]
    public void WriteActions_AreGatedByAdminOnly(string methodName)
    {
        var method = typeof(TracksController).GetMethod(methodName)!;
        var attribute = method.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>().Single();

        Assert.Equal("AdminOnly", attribute.Policy);
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
