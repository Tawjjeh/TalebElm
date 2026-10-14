using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.IntegrationTests;

public class UserUniqueIndexTests : SqliteTestBase
{
    [Fact]
    public async Task SaveChangesAsync_DuplicateEmail_ThrowsUniqueViolation()
    {
        DbContext.Users.Add(
            new User { Name = "User 1", Email = "same@test.com", PasswordHash = "hash-1", JoinedAt = DateTime.UtcNow });
        await SaveChangesAsync();
        ClearTracker();

        DbContext.Users.Add(
            new User { Name = "User 2", Email = "same@test.com", PasswordHash = "hash-2", JoinedAt = DateTime.UtcNow });

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => SaveChangesAsync());

        var sqliteEx = Assert.IsType<SqliteException>(ex.InnerException);
        Assert.Equal(2067, sqliteEx.SqliteExtendedErrorCode);
    }

    [Fact]
    public async Task SaveChangesAsync_CaseVariantEmail_ThrowsUniqueViolation()
    {
        DbContext.Users.Add(
            new User { Name = "User 1", Email = "same@test.com", PasswordHash = "hash-1", JoinedAt = DateTime.UtcNow });
        await SaveChangesAsync();
        ClearTracker();

        DbContext.Users.Add(
            new User { Name = "User 2", Email = "SAME@test.com", PasswordHash = "hash-2", JoinedAt = DateTime.UtcNow });

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => SaveChangesAsync());

        var sqliteEx = Assert.IsType<SqliteException>(ex.InnerException);
        Assert.Equal(2067, sqliteEx.SqliteExtendedErrorCode);
    }

    [Theory]
    [InlineData("", "empty-name@test.com", "hash")]
    [InlineData("   ", "blank-name@test.com", "hash")]
    [InlineData("No Email", "", "hash")]
    [InlineData("No Email", "   ", "hash")]
    [InlineData("No Hash", "no-hash@test.com", "")]
    public async Task SaveChangesAsync_EmptyRequiredField_ThrowsDbUpdateException(
        string name, string email, string passwordHash)
    {
        DbContext.Users.Add(
            new User { Name = name, Email = email, PasswordHash = passwordHash, JoinedAt = DateTime.UtcNow });

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
}
