using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.IntegrationTests;

public class UserProgressPersistenceTests : SqliteTestBase
{
    [Fact]
    public async Task CompletedRequiredProblems_RoundTripsTrueAndFalse()
    {
        var track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Backend track" };
        var module = new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "M1", Order = 1 };
        var completedUser = new User { Id = Guid.NewGuid(), Name = "Completed", Email = "completed@test.com", PasswordHash = "hash-1", JoinedAt = DateTime.UtcNow };
        var freshUser = new User { Id = Guid.NewGuid(), Name = "Fresh", Email = "fresh@test.com", PasswordHash = "hash-2", JoinedAt = DateTime.UtcNow };
        DbContext.Tracks.Add(track);
        DbContext.Modules.Add(module);
        DbContext.Users.AddRange(completedUser, freshUser);
        await SaveChangesAsync();

        var completedUserId = completedUser.Id;
        var freshUserId = freshUser.Id;
        var moduleId = module.Id;
        DbContext.UserProgresses.AddRange(
            new UserProgress
            {
                UserId = completedUserId,
                ModuleId = moduleId,
                IsUnlocked = true,
                PassedExam = true,
                CompletedRequiredProblems = true
            },
            new UserProgress
            {
                UserId = freshUserId,
                ModuleId = moduleId
            });
        await SaveChangesAsync();
        ClearTracker();

        // Read back from SQLite, not from the change tracker, so the stored value is proven.
        var storedCompleted = await DbContext.UserProgresses
            .SingleAsync(p => p.UserId == completedUserId && p.ModuleId == moduleId);
        var storedFresh = await DbContext.UserProgresses
            .SingleAsync(p => p.UserId == freshUserId && p.ModuleId == moduleId);

        Assert.True(storedCompleted.CompletedRequiredProblems);
        Assert.False(storedFresh.CompletedRequiredProblems);
    }
}
