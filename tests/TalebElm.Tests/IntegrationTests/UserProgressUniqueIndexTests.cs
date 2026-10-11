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
        var user = new User { Id = Guid.NewGuid(), Name = "Learner", Email = "learner@test.com", JoinedAt = DateTime.UtcNow };
        DbContext.Tracks.Add(track);
        DbContext.Modules.Add(module);
        DbContext.Users.Add(user);
        await SaveChangesAsync();
        ClearTracker();

        var userId = user.Id;
        var moduleId = module.Id;
        DbContext.UserProgresses.AddRange(
            new UserProgress { UserId = userId, ModuleId = moduleId },
            new UserProgress { UserId = userId, ModuleId = moduleId });

        await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
    }

    [Fact]
    public async Task TwoUsersCanProgressForTheSameModule()
    {
        var track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Backend track" };
        var module = new Module { Id = Guid.NewGuid(), TrackId = track.Id, Title = "M1", Order = 1 };
        var user1 = new User { Id = Guid.NewGuid(), Name = "User 1", Email = "u1@test.com", JoinedAt = DateTime.UtcNow };
        var user2 = new User { Id = Guid.NewGuid(), Name = "User 2", Email = "u2@test.com", JoinedAt = DateTime.UtcNow };
        DbContext.Tracks.Add(track);
        DbContext.Modules.Add(module);
        DbContext.Users.AddRange(user1, user2);
        await SaveChangesAsync();
        ClearTracker();

        var moduleId = module.Id;
        DbContext.UserProgresses.AddRange(
            new UserProgress { UserId = user1.Id, ModuleId = moduleId },
            new UserProgress { UserId = user2.Id, ModuleId = moduleId });

        await SaveChangesAsync();

        Assert.Equal(2, await DbContext.UserProgresses.CountAsync());
    }
}
