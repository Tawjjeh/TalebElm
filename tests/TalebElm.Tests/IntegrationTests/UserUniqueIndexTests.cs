using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.IntegrationTests;

public class UserUniqueIndexTests : SqliteTestBase
{
    [Fact]
    public async Task SaveChangesAsync_DuplicateEmail_ThrowsDbUpdateException()
    {
        DbContext.Users.AddRange(
            new User { Name = "User 1", Email = "same@test.com", PasswordHash = "hash-1", JoinedAt = DateTime.UtcNow },
            new User { Name = "User 2", Email = "same@test.com", PasswordHash = "hash-2", JoinedAt = DateTime.UtcNow });

        await Assert.ThrowsAsync<DbUpdateException>(() => SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_NewUser_PersistsStudentRoleByDefault()
    {
        var user = new User { Name = "Learner", Email = "learner@test.com", PasswordHash = "hash", JoinedAt = DateTime.UtcNow };
        DbContext.Users.Add(user);
        await SaveChangesAsync();
        ClearTracker();

        var stored = await DbContext.Users.SingleAsync(u => u.Id == user.Id);

        Assert.Equal(UserRole.Student, stored.Role);
    }

    [Fact]
    public async Task UserResponse_ShouldNotExposePasswordHash()
    {
        var exposed = typeof(TalebElm.Application.DTOs.UserResponse)
            .GetProperties()
            .Select(p => p.Name);

        Assert.DoesNotContain(exposed, name =>
            name.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Hash", StringComparison.OrdinalIgnoreCase));
    }
}
