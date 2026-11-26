using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TalebElm.Api.Controllers;
using TalebElm.Api.Middlewares;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using System.Security.Claims;
using TalebElm.Domain.Enums;
using DomainNotImplemented = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Tests.IntegrationTests;

public class TracksControllerTests
{
    private readonly Mock<ITrackService> _trackService = new();

    private static TracksController CreateControllerAs(
        ITrackService trackService,
        UserRole role)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Role, role.ToString())],
            authenticationType: "Test");

        return new TracksController(trackService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }

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

    [Fact]
    public async Task Publish_AsNonAdmin_ReturnsForbidden()
    {
        // Arrange: an authenticated user whose role is Student, not Admin.
        var trackId = Guid.NewGuid();
        var controller = CreateControllerAs(_trackService.Object, UserRole.Student);

        // Act
        var actionResult = await controller.PublishAsync(
            trackId,
            CancellationToken.None);

        // Assert
        Assert.IsType<ForbidResult>(actionResult);
        _trackService.Verify(
            s => s.PublishAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never); 
    }

    [Fact]
    public async Task Archive_AsNonAdmin_ReturnsForbidden()
    {
        // Arrange: an authenticated user whose role is Student, not Admin.
        var trackId = Guid.NewGuid();
        var controller = CreateControllerAs(_trackService.Object, UserRole.Student);

        // Act
        var actionResult = await controller.ArchiveAsync(
            trackId,
            CancellationToken.None);

        // Assert
        Assert.IsType<ForbidResult>(actionResult);
        _trackService.Verify(
            s => s.ArchiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
