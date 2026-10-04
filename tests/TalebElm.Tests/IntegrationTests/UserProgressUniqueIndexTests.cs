using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.IntegrationTests;

public class UserProgressUniqueIndexTests : SqliteTestBase
{
    [Fact]
    public async Task SaveChangesAsync_DuplicateUserIdAndModuleId_ThrowsDbUpdateException()
    {
        var userId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        DbContext.UserProgresses.AddRange(
            new UserProgress { UserId = userId, ModuleId = moduleId },
            new UserProgress { UserId = userId, ModuleId = moduleId });
>>>>>>> 1588238 (test: cover merged repository behavior with SQLite)

        await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
    }

    [Fact]
    public async Task TwoUsersCanProgressForTheSameModule()
    {
        var moduleId = Guid.NewGuid();
        DbContext.UserProgresses.AddRange(
            new UserProgress { UserId = Guid.NewGuid(), ModuleId = moduleId },
            new UserProgress { UserId = Guid.NewGuid(), ModuleId = moduleId });
>>>>>>> 1588238 (test: cover merged repository behavior with SQLite)

        await SaveChangesAsync();

        Assert.Equal(2, await DbContext.UserProgresses.CountAsync());
    }
}
