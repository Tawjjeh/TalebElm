using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TalebElm.Api.Middlewares;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Exceptions;
using TalebElm.Infrastructure.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.IntegrationTests;

/// <summary>
/// A race past an application pre-check reaches the database's unique indexes and
/// restrictive foreign keys. Those must come back as conflicts, not 500s (#322).
/// </summary>
public class ConstraintViolationTests : SqliteTestBase
{
    private readonly UnitOfWork _unitOfWork;
    private readonly Track _track;
    private readonly Module _module;
    private readonly User _user;

    public ConstraintViolationTests()
    {
        _unitOfWork = new UnitOfWork(DbContext);
        _track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Backend track" };
        _module = new Module { Id = Guid.NewGuid(), TrackId = _track.Id, Title = "M1", Order = 1 };
        _user = new User { Id = Guid.NewGuid(), Name = "Learner", Email = "learner@test.com", PasswordHash = "hash", JoinedAt = DateTime.UtcNow };
        DbContext.Tracks.Add(_track);
        DbContext.Modules.Add(_module);
        DbContext.Users.Add(_user);
        DbContext.SaveChanges();
        ClearTracker();
    }

    [Fact]
    public async Task DuplicateProgress_ThrowsConflict_AndWritesNothing()
    {
        DbContext.UserProgresses.Add(new UserProgress { UserId = _user.Id, ModuleId = _module.Id });
        await _unitOfWork.SaveChangesAsync();
        ClearTracker();

        DbContext.UserProgresses.Add(new UserProgress { UserId = _user.Id, ModuleId = _module.Id });

        await Assert.ThrowsAsync<ConflictException>(_unitOfWork.SaveChangesAsync);
        await using var check = CreateContext();
        Assert.Equal(1, await check.UserProgresses.CountAsync());
    }

    [Fact]
    public async Task DuplicateEnrollment_ThrowsConflict_AndWritesNothing()
    {
        DbContext.TrackEnrollments.Add(new TrackEnrollment { UserId = _user.Id, TrackId = _track.Id });
        await _unitOfWork.SaveChangesAsync();
        ClearTracker();

        DbContext.TrackEnrollments.Add(new TrackEnrollment { UserId = _user.Id, TrackId = _track.Id });

        await Assert.ThrowsAsync<ConflictException>(_unitOfWork.SaveChangesAsync);
        await using var check = CreateContext();
        Assert.Equal(1, await check.TrackEnrollments.CountAsync());
    }

    [Fact]
    public async Task SecondExamForAModule_ThrowsConflict_AndWritesNothing()
    {
        DbContext.Exams.Add(new Exam { Title = "Exam 1", PassThreshold = 70, ModuleId = _module.Id });
        await _unitOfWork.SaveChangesAsync();
        ClearTracker();

        DbContext.Exams.Add(new Exam { Title = "Exam 2", PassThreshold = 70, ModuleId = _module.Id });

        await Assert.ThrowsAsync<ConflictException>(_unitOfWork.SaveChangesAsync);
        await using var check = CreateContext();
        Assert.Equal("Exam 1", Assert.Single(await check.Exams.ToListAsync()).Title);
    }

    [Fact]
    public async Task DeletingATrackThatStillHasModules_ThrowsConflict_AndDeletesNothing()
    {
        DbContext.Tracks.Remove(await DbContext.Tracks.SingleAsync(t => t.Id == _track.Id));

        await Assert.ThrowsAsync<ConflictException>(_unitOfWork.SaveChangesAsync);
        await using var check = CreateContext();
        Assert.NotNull(await check.Tracks.FindAsync(_track.Id));
    }

    [Fact]
    public async Task UnrelatedDatabaseFailure_StaysADbUpdateException()
    {
        DbContext.Exams.Add(new Exam { Title = null!, PassThreshold = 70, ModuleId = _module.Id });

        // ThrowsAsync checks the exact type, so this is not a ConflictException.
        await Assert.ThrowsAsync<DbUpdateException>(_unitOfWork.SaveChangesAsync);
    }

    [Fact]
    public async Task Middleware_ReturnsConflictWithoutTheProviderMessage()
    {
        DbContext.UserProgresses.AddRange(
            new UserProgress { UserId = _user.Id, ModuleId = _module.Id },
            new UserProgress { UserId = _user.Id, ModuleId = _module.Id });

        var (status, body) = await RunThroughMiddleware(async _ => await _unitOfWork.SaveChangesAsync());

        Assert.Equal(StatusCodes.Status409Conflict, status);
        Assert.DoesNotContain("UNIQUE", body);
        Assert.DoesNotContain("UserProgresses", body);
    }

    [Fact]
    public async Task Middleware_KeepsAnUnrelatedDatabaseFailureAt500()
    {
        DbContext.Exams.Add(new Exam { Title = null!, PassThreshold = 70, ModuleId = _module.Id });

        var (status, body) = await RunThroughMiddleware(async _ => await _unitOfWork.SaveChangesAsync());

        Assert.Equal(StatusCodes.Status500InternalServerError, status);
        Assert.DoesNotContain("NOT NULL", body);
    }

    private static async Task<(int Status, string Body)> RunThroughMiddleware(RequestDelegate next)
    {
        var middleware = new ExceptionHandlingMiddleware(next);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        return (context.Response.StatusCode, await new StreamReader(context.Response.Body).ReadToEndAsync());
    }
}
