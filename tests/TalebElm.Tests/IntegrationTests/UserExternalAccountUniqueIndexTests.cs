using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.IntegrationTests;

public class UserExternalAccountUniqueIndexTests : SqliteTestBase
{
    private async Task<User> CreateUserAsync(string name, string email)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            PasswordHash = "hash",
            JoinedAt = DateTime.UtcNow
        };
        DbContext.Users.Add(user);
        await SaveChangesAsync();
        ClearTracker();
        return user;
    }

    [Fact]
    public async Task UserIdAndPlatform_Duplicate_ThrowsDbUpdateException()
    {
        var user = await CreateUserAsync("Learner 1", "learner1@example.com");

        var account1 = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Platform = ExternalPlatform.Codeforces,
            Handle = "tourist",
            Status = AccountVerificationStatus.Pending
        };

        var account2 = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Platform = ExternalPlatform.Codeforces,
            Handle = "petr",
            Status = AccountVerificationStatus.Pending
        };

        DbContext.UserExternalAccounts.AddRange(account1, account2);
        await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
    }

    [Fact]
    public async Task SameUser_DifferentPlatforms_Succeeds()
    {
        var user = await CreateUserAsync("Learner 2", "learner2@example.com");

        var cfAccount = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Platform = ExternalPlatform.Codeforces,
            Handle = "coder_cf",
            Status = AccountVerificationStatus.Verified
        };

        var ghAccount = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Platform = ExternalPlatform.GitHub,
            Handle = "coder_gh",
            ExternalNumericId = "123456",
            Status = AccountVerificationStatus.Verified
        };

        DbContext.UserExternalAccounts.AddRange(cfAccount, ghAccount);
        await SaveChangesAsync();
        ClearTracker();

        var count = await DbContext.UserExternalAccounts.CountAsync(x => x.UserId == user.Id);
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task VerifiedHandle_DuplicateOnSamePlatform_ThrowsDbUpdateException()
    {
        var user1 = await CreateUserAsync("Learner 3", "learner3@example.com");
        var user2 = await CreateUserAsync("Learner 4", "learner4@example.com");

        var account1 = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user1.Id,
            Platform = ExternalPlatform.Codeforces,
            Handle = "same_verified_handle",
            Status = AccountVerificationStatus.Verified
        };

        var account2 = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user2.Id,
            Platform = ExternalPlatform.Codeforces,
            Handle = "same_verified_handle",
            Status = AccountVerificationStatus.Verified
        };

        DbContext.UserExternalAccounts.AddRange(account1, account2);
        await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
    }

    [Fact]
    public async Task PendingHandles_DuplicateOnSamePlatform_AreAllowed()
    {
        // Decision 33 & Issue #385: Pending claims never block a real owner
        var user1 = await CreateUserAsync("Learner 5", "learner5@example.com");
        var user2 = await CreateUserAsync("Learner 6", "learner6@example.com");

        var account1 = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user1.Id,
            Platform = ExternalPlatform.Codeforces,
            Handle = "contested_handle",
            Status = AccountVerificationStatus.Pending
        };

        var account2 = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user2.Id,
            Platform = ExternalPlatform.Codeforces,
            Handle = "contested_handle",
            Status = AccountVerificationStatus.Pending
        };

        DbContext.UserExternalAccounts.AddRange(account1, account2);
        await SaveChangesAsync();
        ClearTracker();

        var count = await DbContext.UserExternalAccounts
            .CountAsync(x => x.Handle == "contested_handle" && x.Status == AccountVerificationStatus.Pending);
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task VerifiedAndPendingHandles_SameHandle_AreAllowed()
    {
        var user1 = await CreateUserAsync("Learner 7", "learner7@example.com");
        var user2 = await CreateUserAsync("Learner 8", "learner8@example.com");

        var verified = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user1.Id,
            Platform = ExternalPlatform.Codeforces,
            Handle = "shared_handle",
            Status = AccountVerificationStatus.Verified
        };

        var pending = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user2.Id,
            Platform = ExternalPlatform.Codeforces,
            Handle = "shared_handle",
            Status = AccountVerificationStatus.Pending
        };

        DbContext.UserExternalAccounts.AddRange(verified, pending);
        await SaveChangesAsync();
        ClearTracker();

        var count = await DbContext.UserExternalAccounts.CountAsync(x => x.Handle == "shared_handle");
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task VerifiedHandles_DifferentPlatforms_SameHandle_AreAllowed()
    {
        var user1 = await CreateUserAsync("Learner 9", "learner9@example.com");
        var user2 = await CreateUserAsync("Learner 10", "learner10@example.com");

        var cfAccount = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user1.Id,
            Platform = ExternalPlatform.Codeforces,
            Handle = "cross_platform_handle",
            Status = AccountVerificationStatus.Verified
        };

        var ghAccount = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user2.Id,
            Platform = ExternalPlatform.GitHub,
            Handle = "cross_platform_handle",
            ExternalNumericId = "998877",
            Status = AccountVerificationStatus.Verified
        };

        DbContext.UserExternalAccounts.AddRange(cfAccount, ghAccount);
        await SaveChangesAsync();
        ClearTracker();

        var count = await DbContext.UserExternalAccounts.CountAsync(x => x.Handle == "cross_platform_handle");
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task ExternalNumericId_DuplicateOnSamePlatform_ThrowsDbUpdateException()
    {
        var user1 = await CreateUserAsync("Learner 11", "learner11@example.com");
        var user2 = await CreateUserAsync("Learner 12", "learner12@example.com");

        var ghAccount1 = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user1.Id,
            Platform = ExternalPlatform.GitHub,
            Handle = "dev_one",
            ExternalNumericId = "424242",
            Status = AccountVerificationStatus.Verified
        };

        var ghAccount2 = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user2.Id,
            Platform = ExternalPlatform.GitHub,
            Handle = "dev_two",
            ExternalNumericId = "424242",
            Status = AccountVerificationStatus.Verified
        };

        DbContext.UserExternalAccounts.AddRange(ghAccount1, ghAccount2);
        await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
    }

    [Fact]
    public async Task ExternalNumericId_Null_MultipleAllowedOnSamePlatform()
    {
        var user1 = await CreateUserAsync("Learner 13", "learner13@example.com");
        var user2 = await CreateUserAsync("Learner 14", "learner14@example.com");

        var cfAccount1 = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user1.Id,
            Platform = ExternalPlatform.Codeforces,
            Handle = "handle_alpha",
            ExternalNumericId = null,
            Status = AccountVerificationStatus.Pending
        };

        var cfAccount2 = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user2.Id,
            Platform = ExternalPlatform.Codeforces,
            Handle = "handle_beta",
            ExternalNumericId = null,
            Status = AccountVerificationStatus.Pending
        };

        DbContext.UserExternalAccounts.AddRange(cfAccount1, cfAccount2);
        await SaveChangesAsync();
        ClearTracker();

        var count = await DbContext.UserExternalAccounts.CountAsync(x => x.ExternalNumericId == null);
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task EmptyHandle_ThrowsDbUpdateException_DueToCheckConstraint()
    {
        var user = await CreateUserAsync("Learner 15", "learner15@example.com");

        var account = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Platform = ExternalPlatform.Codeforces,
            Handle = "   ",
            Status = AccountVerificationStatus.Pending
        };

        DbContext.UserExternalAccounts.Add(account);
        await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
    }

    [Fact]
    public async Task DeleteUser_WithAssociatedExternalAccount_ThrowsDueToRestrictDelete()
    {
        var user = await CreateUserAsync("Learner 16", "learner16@example.com");

        var account = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Platform = ExternalPlatform.Codeforces,
            Handle = "learner_handle",
            Status = AccountVerificationStatus.Verified
        };

        DbContext.UserExternalAccounts.Add(account);
        await SaveChangesAsync();
        ClearTracker();

        var userToDelete = await DbContext.Users.FindAsync(user.Id);
        Assert.NotNull(userToDelete);
        DbContext.Users.Remove(userToDelete!);

        await Assert.ThrowsAsync<DbUpdateException>(SaveChangesAsync);
    }
}
