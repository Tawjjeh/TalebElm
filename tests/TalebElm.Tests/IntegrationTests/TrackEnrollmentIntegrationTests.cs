using Microsoft.EntityFrameworkCore;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Exceptions;
using TalebElm.Infrastructure.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.IntegrationTests;

public class TrackEnrollmentIntegrationTests : SqliteTestBase
{
    [Fact]
    public async Task EnrollAsync_IgnoresOtherPairsAndRejectsTheExactDuplicate()
    {
        var learner = new User { Name = "Learner", Email = "learner@example.invalid", PasswordHash = "test-hash" };
        var otherLearner = new User { Name = "Other learner", Email = "other@example.invalid", PasswordHash = "test-hash" };
        var track = new Track { Name = "Track", Description = "Test track", Status = TrackStatus.Published };
        var otherTrack = new Track { Name = "Other track", Description = "Test track", Status = TrackStatus.Published };
        var module = new Module { TrackId = track.Id, Title = "First module", Order = 1 };
        DbContext.AddRange(learner, otherLearner, track, otherTrack, module,
            new TrackEnrollment { UserId = learner.Id, TrackId = otherTrack.Id },
            new TrackEnrollment { UserId = otherLearner.Id, TrackId = track.Id });
        await SaveChangesAsync();
        ClearTracker();
        var service = new TrackEnrollmentService(new UnitOfWork(DbContext));

        var enrollment = await service.EnrollAsync(learner.Id, track.Id);

        var trackedEnrollment = Assert.Single(DbContext.ChangeTracker.Entries<TrackEnrollment>());
        Assert.Equal(enrollment.Id, trackedEnrollment.Entity.Id);
        ClearTracker();
        var progress = await DbContext.UserProgresses.SingleAsync();
        Assert.Equal((learner.Id, module.Id, true), (progress.UserId, progress.ModuleId, progress.IsUnlocked));
        await Assert.ThrowsAsync<ConflictException>(() => service.EnrollAsync(learner.Id, track.Id));
        Assert.Equal(3, await DbContext.TrackEnrollments.CountAsync());
        Assert.Equal(1, await DbContext.UserProgresses.CountAsync());
    }
}
