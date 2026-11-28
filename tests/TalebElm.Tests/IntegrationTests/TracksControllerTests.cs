using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TalebElm.Api.Controllers;
using TalebElm.Api.Middlewares;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Exceptions;
using DomainNotImplemented = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Tests.IntegrationTests;

public class TracksControllerTests
{
    private readonly Mock<ITrackService> _trackService = new();
    private readonly Mock<ITrackEnrollmentService> _trackEnrollmentService = new();

    private TracksController Controller() => new(_trackService.Object, _trackEnrollmentService.Object);

    private TracksController ControllerAsUser(Guid userId)
    {
        var controller = Controller();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
                    "TestAuthentication"))
            }
        };
        return controller;
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

        var controller = new TracksController(_trackService.Object, _trackEnrollmentService.Object);

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
        var controller = new TracksController(_trackService.Object, _trackEnrollmentService.Object);

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
    public async Task GetById_ReturnsOkWithTrackDetailResponse()
    {
        var trackId = Guid.NewGuid();
        var expectedResponse = new TrackDetailResponse(
            trackId,
            "Backend .NET",
            "Description",
            1,
            [new ModuleResponse(Guid.NewGuid(), "M1", "Summary", 1, trackId)]);

        _trackService
            .Setup(s => s.GetByIdAsync(trackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var actionResult = await Controller().GetById(trackId);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var result = Assert.IsType<TrackDetailResponse>(okResult.Value);
        Assert.Equal(expectedResponse.Id, result.Id);
        Assert.Single(result.Modules);
    }

    [Fact]
    public async Task GetById_WhenTrackIsMissing_MapsTo404ThroughMiddleware()
    {
        var trackId = Guid.NewGuid();
        _trackService
            .Setup(s => s.GetByIdAsync(trackId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Track with id '{trackId}' was not found."));

        var controller = Controller();
        var middleware = new ExceptionHandlingMiddleware(async _ => await controller.GetById(trackId));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("\"status\":404", body);
    }

    [Fact]
    public async Task Enroll_WithTestClaims_Returns201WithEnrollmentResponse()
    {
        var userId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var expectedResponse = new EnrollmentResponse(Guid.NewGuid(), trackId, DateTimeOffset.UtcNow);

        _trackEnrollmentService
            .Setup(s => s.EnrollAsync(userId, trackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var actionResult = await ControllerAsUser(userId).Enroll(trackId);

        var createdResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        var result = Assert.IsType<EnrollmentResponse>(createdResult.Value);
        Assert.Equal(expectedResponse.Id, result.Id);
        Assert.Equal(trackId, result.TrackId);
    }

    [Fact]
    public async Task Enroll_WithoutAuthenticationClaims_Returns401()
    {
        var actionResult = await Controller().Enroll(Guid.NewGuid());

        Assert.IsType<UnauthorizedResult>(actionResult);
        _trackEnrollmentService.Verify(
            s => s.EnrollAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Enroll_WithNonGuidClaim_Returns401()
    {
        var controller = Controller();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, "not-a-guid")],
                    "TestAuthentication"))
            }
        };

        var actionResult = await controller.Enroll(Guid.NewGuid());

        Assert.IsType<UnauthorizedResult>(actionResult);
    }

    [Fact]
    public async Task Enroll_WhenAlreadyEnrolled_MapsTo409ThroughMiddleware()
    {
        var userId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        _trackEnrollmentService
            .Setup(s => s.EnrollAsync(userId, trackId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("The user has already enrolled in this track."));

        var controller = ControllerAsUser(userId);
        var middleware = new ExceptionHandlingMiddleware(async _ => await controller.Enroll(trackId));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("\"status\":409", body);
    }

    [Fact]
    public async Task Enroll_WhenTrackIsMissing_MapsTo404ThroughMiddleware()
    {
        var userId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        _trackEnrollmentService
            .Setup(s => s.EnrollAsync(userId, trackId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Track with id '{trackId}' was not found."));

        var controller = ControllerAsUser(userId);
        var middleware = new ExceptionHandlingMiddleware(async _ => await controller.Enroll(trackId));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("\"status\":404", body);
    }
}
