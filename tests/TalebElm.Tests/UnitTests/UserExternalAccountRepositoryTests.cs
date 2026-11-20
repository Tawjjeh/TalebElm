using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Infrastructure.Persistence.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class UserExternalAccountRepositoryTests : SqliteTestBase
{
    private readonly UserExternalAccountRepository _repository;

    public UserExternalAccountRepositoryTests() => _repository = new UserExternalAccountRepository(DbContext);

    private async Task<User> AddUserAsync(string email)
    {
        var user = new User { Name = "Test", Email = email, PasswordHash = "hash" };
        DbContext.Users.Add(user);
        await SaveChangesAsync();
        return user;
    }

    private static UserExternalAccount NewAccount(
        Guid userId,
        string handle,
        ExternalPlatform platform = ExternalPlatform.Codeforces,
        AccountVerificationStatus status = AccountVerificationStatus.Pending)
        => new()
        {
            UserId = userId,
            Platform = platform,
            Handle = handle,
            Status = status
        };

    [Fact]
    public async Task GetByUserAndPlatformAsync_ReturnsMatchingAccount()
    {
        var user = await AddUserAsync("a@test.com");
        var account = NewAccount(user.Id, "tourist");
        DbContext.UserExternalAccounts.Add(account);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetByUserAndPlatformAsync(user.Id, ExternalPlatform.Codeforces);

        Assert.NotNull(result);
        Assert.Equal(account.Id, result!.Id);
    }

    [Fact]
    public async Task GetByUserAndPlatformAsync_WhenPlatformDiffers_ReturnsNull()
    {
        var user = await AddUserAsync("b@test.com");
        DbContext.UserExternalAccounts.Add(NewAccount(user.Id, "tourist"));
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetByUserAndPlatformAsync(user.Id, ExternalPlatform.GitHub);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByPlatformAndHandleAsync_ReturnsMatchingAccount()
    {
        var user = await AddUserAsync("c@test.com");
        var account = NewAccount(user.Id, "tourist");
        DbContext.UserExternalAccounts.Add(account);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetByPlatformAndHandleAsync(ExternalPlatform.Codeforces, "tourist");

        Assert.NotNull(result);
        Assert.Equal(account.Id, result!.Id);
    }

    [Fact]
    public async Task GetByPlatformAndHandleAsync_WhenHandleIsMissing_ReturnsNull()
    {
        var result = await _repository.GetByPlatformAndHandleAsync(ExternalPlatform.Codeforces, "nobody");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetVerifiedByPlatformAndHandleAsync_ReturnsVerifiedAccount()
    {
        var user = await AddUserAsync("e@test.com");
        var account = NewAccount(user.Id, "tourist", status: AccountVerificationStatus.Verified);
        DbContext.UserExternalAccounts.Add(account);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetVerifiedByPlatformAndHandleAsync(ExternalPlatform.Codeforces, "tourist");

        Assert.NotNull(result);
        Assert.Equal(account.Id, result!.Id);
    }

    [Fact]
    public async Task GetVerifiedByPlatformAndHandleAsync_IgnoresPendingAccounts()
    {
        var user = await AddUserAsync("f@test.com");
        DbContext.UserExternalAccounts.Add(NewAccount(user.Id, "tourist"));
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetVerifiedByPlatformAndHandleAsync(ExternalPlatform.Codeforces, "tourist");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAccountIsMissing_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_DoesNotPersist_UntilSave()
    {
        var user = await AddUserAsync("d@test.com");
        var account = NewAccount(user.Id, "tourist");

        await _repository.AddAsync(account);

        await using (var before = CreateContext())
            Assert.Equal(0, before.UserExternalAccounts.Count(a => a.Id == account.Id));

        await SaveChangesAsync();

        await using (var after = CreateContext())
            Assert.Equal(1, after.UserExternalAccounts.Count(a => a.Id == account.Id));
    }
}
