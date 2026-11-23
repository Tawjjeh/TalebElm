using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TalebElm.Api.Controllers;
using TalebElm.Api.Middlewares;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Infrastructure.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.IntegrationTests;

/// <summary>
/// Runs the progress endpoints against the real service and a real SQLite
/// database, with test authentication claims standing in for the caller.
/// </summary>
public class UserProgressControllerTests : SqliteTestBase
{
    private readonly UserProgressService _service;
    private readonly User _learner;
    private readonly User _otherLearner;
    private readonly User _learnerWithoutProgress;
    private readonly Track _track;
    private readonly Module _firstModule;
    private readonly Module _secondModule;

    public UserProgressControllerTests()
    {
        _service = new UserProgressService(new UnitOfWork(DbContext));

        //. Creating Test Values to be tested 
        _track = new Track { Name = "Backend", Description = "Backend track", Status = TrackStatus.Published };
        _firstModule = new Module { Track = _track, Title = "M1", Order = 1 };
        _secondModule = new Module { Track = _track, Title = "M2", Order = 2 };
        _learner = new User { Name = "Learner", Email = "learner@test.com", PasswordHash = "hash" };
        _otherLearner = new User { Name = "Other", Email = "other@test.com", PasswordHash = "hash" };
        _learnerWithoutProgress = new User { Name = "New", Email = "new@test.com", PasswordHash = "hash" };

        //.in memory
        DbContext.Tracks.Add(_track);
        DbContext.Modules.AddRange(_firstModule, _secondModule);
        DbContext.Users.AddRange(_learner, _otherLearner, _learnerWithoutProgress);

        DbContext.UserProgresses.AddRange(
            new UserProgress
            {
                UserId = _learner.Id,
                ModuleId = _firstModule.Id,
                IsUnlocked = true,
                PassedExam = true,
                Score = 80
            },
            new UserProgress
            {
                UserId = _learner.Id,
                ModuleId = _secondModule.Id,
                IsUnlocked = false
            },
            new UserProgress
            {
                UserId = _otherLearner.Id,
                ModuleId = _firstModule.Id,
                IsUnlocked = true,
                PassedExam = true,
                Score = 100
            });
        DbContext.SaveChanges();
        ClearTracker();
    }

    private ProgressController ControllerFor(Guid userId)
        => new ProgressController(_service).AsUser(userId);

    private static IReadOnlyList<ProgressResponse> ProgressFrom(IActionResult actionResult)
        => Assert.IsAssignableFrom<IReadOnlyList<ProgressResponse>>(
            Assert.IsType<OkObjectResult>(actionResult).Value);

    [Fact]
    public async Task MyProgress_WithTestClaims_ReturnsOnlyCallerProgress()
    {
        var controller = ControllerFor(_learner.Id);

        var progress = ProgressFrom(await controller.MyProgress());

        Assert.Equal(2, progress.Count);
        Assert.Contains(progress, p => p.ModuleId == _firstModule.Id && p.PassedExam && p.Score == 80);
        Assert.Contains(progress, p => p.ModuleId == _secondModule.Id && !p.IsUnlocked);
        Assert.DoesNotContain(progress, p => p.Score == 100);
    }

    [Fact]
    public async Task GetByTrack_WithTestClaims_ReturnsOnlyCallerProgressInTheTrack()
    {
        var controller = ControllerFor(_otherLearner.Id);

        var progress = ProgressFrom(await controller.GetByTrack(_track.Id));

        var single = Assert.Single(progress);
        Assert.Equal(_firstModule.Id, single.ModuleId);
        Assert.Equal(100, single.Score);
    }

    [Fact]
    public async Task MyProgress_WithNoProgress_ReturnsEmptyList()
    {
        var controller = ControllerFor(_learnerWithoutProgress.Id);

        var progress = ProgressFrom(await controller.MyProgress());

        Assert.Empty(progress);
    }

    [Fact]
    public async Task GetByTrack_WithUnknownTrack_Returns404ThroughMiddleware()
    {
        var controller = ControllerFor(_learner.Id);
        var middleware = new ExceptionHandlingMiddleware(async _ => await controller.GetByTrack(Guid.NewGuid()));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
    }

    [Fact]
    public async Task MyProgress_WithoutAuthenticationClaims_Returns401()
    {
        var controller = new ProgressController(_service).AsAnonymous();

        Assert.IsType<UnauthorizedResult>(await controller.MyProgress());
    }
}
