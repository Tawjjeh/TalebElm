using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.IntegrationTests;

public class UserProgressUniqueIndexTests : SqliteTestBase
{
    [Fact]
    public async Task SaveChangesAsync_DuplicateUserIdAndModuleId_ThrowsDbUpdateException()
    {
        var track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Backend track" };
        var module = new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "M1", Order = 1 };
        var user = new User { Id = Guid.NewGuid(), Name = "Learner", Email = "learner@test.com", PasswordHash = "hash", JoinedAt = DateTime.UtcNow };
        DbContext.Tracks.Add(track);
        DbContext.Modules.Add(module);
        DbContext.Users.Add(user);
        DbContext.TrackEnrollments.Add(new TrackEnrollment { UserId = user.Id, TrackId = track.Id });
        await SaveChangesAsync();
        ClearTracker();

        var userId = user.Id;
        var moduleId = module.Id;
        DbContext.UserProgresses.AddRange(
            new UserProgress { UserId = userId, ModuleId = moduleId, TrackId = track.Id },
            new UserProgress { UserId = userId, ModuleId = moduleId, TrackId = track.Id });

        await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
    }

    [Fact]
    public async Task TwoUsersCanProgressForTheSameModule()
    {
        var track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Backend track" };
        var module = new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "M1", Order = 1 };
        var user1 = new User { Id = Guid.NewGuid(), Name = "User 1", Email = "u1@test.com", PasswordHash = "hash-1", JoinedAt = DateTime.UtcNow };
        var user2 = new User { Id = Guid.NewGuid(), Name = "User 2", Email = "u2@test.com", PasswordHash = "hash-2", JoinedAt = DateTime.UtcNow };
        DbContext.Tracks.Add(track);
        DbContext.Modules.Add(module);
        DbContext.Users.AddRange(user1, user2);
        DbContext.TrackEnrollments.AddRange(
            new TrackEnrollment { UserId = user1.Id, TrackId = track.Id },
            new TrackEnrollment { UserId = user2.Id, TrackId = track.Id });
        await SaveChangesAsync();
        ClearTracker();

        var moduleId = module.Id;
        DbContext.UserProgresses.AddRange(
            new UserProgress { UserId = user1.Id, ModuleId = moduleId, TrackId = track.Id },
            new UserProgress { UserId = user2.Id, ModuleId = moduleId, TrackId = track.Id });

        await SaveChangesAsync();

        Assert.Equal(2, await DbContext.UserProgresses.CountAsync());
    }
}
