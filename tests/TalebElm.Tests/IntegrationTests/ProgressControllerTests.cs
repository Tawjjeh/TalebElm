using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TalebElm.Api.Controllers;
using TalebElm.Api.Middlewares;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Exceptions;

namespace TalebElm.Tests.IntegrationTests;

public class ProgressControllerTests
{
    private readonly Mock<IUserProgressService> _progressService = new();
    private readonly Guid _adminId = Guid.NewGuid();
    private readonly Guid _learnerId = Guid.NewGuid();
    private readonly Guid _moduleId = Guid.NewGuid();

    private ProgressController ControllerWithClaims(string? adminClaim = null)
    {
        var claims = adminClaim is null
            ? Array.Empty<Claim>()
            : new[] { new Claim(ClaimTypes.NameIdentifier, adminClaim) };
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
        };
        var controller = new ProgressController(_progressService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
        return controller;
    }

    [Fact]
    public async Task Unlock_WithAdminClaims_ReturnsOkWithUnlockedProgress()
    {
        var request = new AdminUnlockModuleRequest(_learnerId, _moduleId);
        var expected = new ProgressResponse(_moduleId, IsUnlocked: true, PassedExam: false, Score: 0);
        _progressService.Setup(s => s.AdminUnlockAsync(_adminId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await ControllerWithClaims(_adminId.ToString()).Unlock(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
        _progressService.Verify(s => s.AdminUnlockAsync(_adminId, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Unlock_WithoutClaims_Returns401()
    {
        var request = new AdminUnlockModuleRequest(_learnerId, _moduleId);

        var result = await ControllerWithClaims().Unlock(request);

        Assert.IsType<UnauthorizedResult>(result);
        _progressService.Verify(s => s.AdminUnlockAsync(It.IsAny<Guid>(), It.IsAny<AdminUnlockModuleRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Unlock_WithNonGuidClaim_Returns401()
    {
        var request = new AdminUnlockModuleRequest(_learnerId, _moduleId);

        var result = await ControllerWithClaims("not-a-guid").Unlock(request);

        Assert.IsType<UnauthorizedResult>(result);
        _progressService.Verify(s => s.AdminUnlockAsync(It.IsAny<Guid>(), It.IsAny<AdminUnlockModuleRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Unlock_IsGatedByAdminOnlyPolicy()
    {
        // The 403 for a non-Admin caller is enforced by the authorization policy,
        // which direct controller invocation does not run; assert the gate is declared.
        var method = typeof(ProgressController).GetMethod(nameof(ProgressController.Unlock))!;
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        Assert.Equal("AdminOnly", authorize.Policy);
    }

    [Fact]
    public async Task Middleware_MapsNotFoundExceptionTo404Problem()
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new NotFoundException("Learner was not found."));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("\"status\":404", body);
    }
}
