using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.IntegrationTests;

public class CodeforcesChallengeServicePersistenceTests : SqliteTestBase
{
    private static readonly DateTimeOffset Now = new(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ExpectedExpiry = Now.AddMinutes(15);

    private CodeforcesChallengeService CreateService(ILogger<UnitOfWork>? logger = null, IUnitOfWork? unitOfWork = null)
        => new(unitOfWork ?? new UnitOfWork(DbContext, logger), new FixedTimeProvider(Now));

    private async Task<User> AddUserAsync(string email)
    {
        var user = new User { Name = "Test", Email = email, PasswordHash = "hash" };
        DbContext.Users.Add(user);
        await SaveChangesAsync();
        ClearTracker();
        return user;
    }

    private async Task<UserExternalAccount> AddAccountAsync(
        Guid userId,
        string handle,
        AccountVerificationStatus status,
        string? token = null,
        DateTime? expiry = null)
    {
        var account = new UserExternalAccount
        {
            UserId = userId,
            Platform = ExternalPlatform.Codeforces,
            Handle = handle,
            Status = status,
            VerificationToken = token,
            TokenExpiresAtUtc = expiry
        };
        DbContext.UserExternalAccounts.Add(account);
        await SaveChangesAsync();
        ClearTracker();
        return account;
    }

    private async Task<List<UserExternalAccount>> ReadAccountsAsync(Guid userId)
    {
        await using var context = CreateContext();
        return await context.UserExternalAccounts
            .Where(a => a.UserId == userId && a.Platform == ExternalPlatform.Codeforces)
            .ToListAsync();
    }

    [Fact]
    public async Task GenerateChallengeAsync_NewAccount_PersistsPendingAccount()
    {
        var user = await AddUserAsync("new@example.com");

        var response = await CreateService().GenerateChallengeAsync(user.Id, "  tourist  ");

        var account = Assert.Single(await ReadAccountsAsync(user.Id));
        Assert.Equal(response.Id, account.Id);
        Assert.Equal(user.Id, account.UserId);
        Assert.Equal(ExternalPlatform.Codeforces, account.Platform);
        Assert.Equal("tourist", account.Handle);
        Assert.Equal(AccountVerificationStatus.Pending, account.Status);
        Assert.Equal(response.VerificationToken, account.VerificationToken);
        Assert.Equal(ExpectedExpiry.UtcDateTime, account.TokenExpiresAtUtc!.Value);
        Assert.Equal(ExpectedExpiry, response.TokenExpiresAt);
    }

    [Fact]
    public async Task GenerateChallengeAsync_ExistingPendingAccount_UpdatesInPlace()
    {
        var user = await AddUserAsync("overwrite@example.com");
        var existing = await AddAccountAsync(
            user.Id, "old_handle", AccountVerificationStatus.Pending, "old_token", Now.AddDays(-1).UtcDateTime);

        var response = await CreateService().GenerateChallengeAsync(user.Id, "  new_handle  ");

        var account = Assert.Single(await ReadAccountsAsync(user.Id));
        Assert.Equal(existing.Id, account.Id);
        Assert.Equal(user.Id, account.UserId);
        Assert.Equal(ExternalPlatform.Codeforces, account.Platform);
        Assert.Equal("new_handle", account.Handle);
        Assert.Equal(AccountVerificationStatus.Pending, account.Status);
        Assert.Equal(response.VerificationToken, account.VerificationToken);
        Assert.NotEqual("old_token", account.VerificationToken);
        Assert.Equal(ExpectedExpiry.UtcDateTime, account.TokenExpiresAtUtc!.Value);
    }

    [Fact]
    public async Task GenerateChallengeAsync_ExpiredAccount_ResetsToPending()
    {
        var user = await AddUserAsync("expired@example.com");
        var existing = await AddAccountAsync(
            user.Id, "old_handle", AccountVerificationStatus.Expired, "old_token", Now.AddDays(-1).UtcDateTime);

        await CreateService().GenerateChallengeAsync(user.Id, "new_handle");

        var account = Assert.Single(await ReadAccountsAsync(user.Id));
        Assert.Equal(existing.Id, account.Id);
        Assert.Equal(AccountVerificationStatus.Pending, account.Status);
        Assert.Equal("new_handle", account.Handle);
        Assert.Equal(ExpectedExpiry.UtcDateTime, account.TokenExpiresAtUtc!.Value);
    }

    [Fact]
    public async Task GenerateChallengeAsync_OtherUsersVerifiedHandle_ThrowsConflictWithoutChanges()
    {
        var target = await AddUserAsync("target-a@example.com");
        var other = await AddUserAsync("other-a@example.com");
        var verified = await AddAccountAsync(
            other.Id, "tourist", AccountVerificationStatus.Verified, "verified_token", Now.UtcDateTime);

        await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().GenerateChallengeAsync(target.Id, "tourist"));

        Assert.Empty(await ReadAccountsAsync(target.Id));
        var unchanged = Assert.Single(await ReadAccountsAsync(other.Id));
        Assert.Equal(verified.Id, unchanged.Id);
        Assert.Equal("tourist", unchanged.Handle);
        Assert.Equal(AccountVerificationStatus.Verified, unchanged.Status);
        Assert.Equal("verified_token", unchanged.VerificationToken);
    }

    [Fact]
    public async Task GenerateChallengeAsync_UserWithVerifiedCodeforces_ThrowsConflictWithoutChanges()
    {
        var user = await AddUserAsync("target-b@example.com");
        var verified = await AddAccountAsync(
            user.Id, "old_handle", AccountVerificationStatus.Verified, "verified_token", Now.UtcDateTime);

        await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().GenerateChallengeAsync(user.Id, "new_handle"));

        var unchanged = Assert.Single(await ReadAccountsAsync(user.Id));
        Assert.Equal(verified.Id, unchanged.Id);
        Assert.Equal("old_handle", unchanged.Handle);
        Assert.Equal(AccountVerificationStatus.Verified, unchanged.Status);
        Assert.Equal("verified_token", unchanged.VerificationToken);
    }

    [Fact]
    public async Task GenerateChallengeAsync_OtherUsersPendingSameHandle_DoesNotBlock()
    {
        var target = await AddUserAsync("target-c@example.com");
        var other = await AddUserAsync("other-c@example.com");
        await AddAccountAsync(
            other.Id, "tourist", AccountVerificationStatus.Pending, "other_token", Now.UtcDateTime);

        var response = await CreateService().GenerateChallengeAsync(target.Id, "tourist");

        Assert.NotNull(response.VerificationToken);
        Assert.Equal("tourist", Assert.Single(await ReadAccountsAsync(target.Id)).Handle);
        Assert.Equal("other_token", Assert.Single(await ReadAccountsAsync(other.Id)).VerificationToken);
    }

    [Theory]
    [InlineData("Tourist", "tourist")]
    [InlineData("tourist", "TOURIST")]
    public async Task GenerateChallengeAsync_VerifiedHandleComparisonIsCaseInsensitive(
        string verifiedHandle,
        string attemptedHandle)
    {
        var target = await AddUserAsync($"target-{attemptedHandle.ToLowerInvariant()}@example.com");
        var other = await AddUserAsync($"other-{verifiedHandle.ToLowerInvariant()}@example.com");
        var verified = await AddAccountAsync(other.Id, verifiedHandle, AccountVerificationStatus.Verified);

        await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().GenerateChallengeAsync(target.Id, attemptedHandle));

        Assert.Empty(await ReadAccountsAsync(target.Id));
        var unchanged = Assert.Single(await ReadAccountsAsync(other.Id));
        Assert.Equal(verified.Id, unchanged.Id);
        Assert.Equal(verifiedHandle, unchanged.Handle);
        Assert.Equal(AccountVerificationStatus.Verified, unchanged.Status);
    }

    [Fact]
    public async Task GenerateChallengeAsync_WhenSaveFails_CommitsNothing()
    {
        var user = await AddUserAsync("failure@example.com");
        var failingUnitOfWork = new FailingSaveUnitOfWork(new UnitOfWork(DbContext));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => new CodeforcesChallengeService(failingUnitOfWork, new FixedTimeProvider(Now))
                .GenerateChallengeAsync(user.Id, "tourist"));

        Assert.Empty(await ReadAccountsAsync(user.Id));
    }

    [Fact]
    public async Task GenerateChallengeAsync_DoesNotLogTheToken()
    {
        var user = await AddUserAsync("logging@example.com");
        var logger = new CapturingLogger<UnitOfWork>();

        var response = await CreateService(logger).GenerateChallengeAsync(user.Id, "tourist");

        Assert.DoesNotContain(logger.Entries,
            entry => entry.Contains(response.VerificationToken!, StringComparison.Ordinal));
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Entries.Add(formatter(state, exception));
    }

    private sealed class FailingSaveUnitOfWork(IUnitOfWork inner) : IUnitOfWork
    {
        public IUserRepository Users => inner.Users;
        public ITrackRepository Tracks => inner.Tracks;
        public IModuleRepository Modules => inner.Modules;
        public ILessonRepository Lessons => inner.Lessons;
        public IExamRepository Exams => inner.Exams;
        public IUserProgressRepository UserProgresses => inner.UserProgresses;
        public ITrackEnrollmentRepository TrackEnrollments => inner.TrackEnrollments;
        public ILessonResourceRepository LessonResources => inner.LessonResources;
        public ITechnologyRepository Technologies => inner.Technologies;
        public IResourceRepository Resources => inner.Resources;
        public IUserExternalAccountRepository UserExternalAccounts => inner.UserExternalAccounts;

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => throw new InvalidOperationException("Simulated save failure.");
    }
}
