using Moq;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class CodeforcesChallengeServiceTests
{
    private const string TokenPrefix = "taleb-elm-verify-";
    private static readonly DateTimeOffset Now = new(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);

    private sealed class Fixture
    {
        public Mock<IUnitOfWork> Uow { get; } = new();
        public Mock<IUserRepository> Users { get; } = new();
        public Mock<IUserExternalAccountRepository> Accounts { get; } = new();
        public List<UserExternalAccount> Added { get; } = new();
        public CodeforcesChallengeService Service { get; }

        public Fixture(TimeProvider? timeProvider = null)
        {
            Users.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken _) => new User
                {
                    Id = id,
                    Name = "Test",
                    Email = $"{id}@test.com",
                    PasswordHash = "hash"
                });

            Accounts.Setup(r => r.GetVerifiedByPlatformAndHandleAsync(
                    It.IsAny<ExternalPlatform>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserExternalAccount?)null);

            Accounts.Setup(r => r.GetByUserAndPlatformAsync(
                    It.IsAny<Guid>(), It.IsAny<ExternalPlatform>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserExternalAccount?)null);

            Accounts.Setup(r => r.AddAsync(It.IsAny<UserExternalAccount>(), It.IsAny<CancellationToken>()))
                .Callback<UserExternalAccount, CancellationToken>((account, _) => Added.Add(account))
                .Returns(Task.CompletedTask);

            Uow.Setup(u => u.Users).Returns(Users.Object);
            Uow.Setup(u => u.UserExternalAccounts).Returns(Accounts.Object);
            Uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            Service = new CodeforcesChallengeService(Uow.Object, timeProvider);
        }
    }

    [Fact]
    public async Task GenerateChallengeAsync_NullHandle_ThrowsValidationException()
    {
        var fixture = new Fixture();

        await Assert.ThrowsAsync<ValidationException>(
            () => fixture.Service.GenerateChallengeAsync(Guid.NewGuid(), null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GenerateChallengeAsync_BlankHandle_ThrowsValidationException(string handle)
    {
        var fixture = new Fixture();

        await Assert.ThrowsAsync<ValidationException>(
            () => fixture.Service.GenerateChallengeAsync(Guid.NewGuid(), handle));
    }

    [Fact]
    public async Task GenerateChallengeAsync_HandleLongerThan64_ThrowsValidationException()
    {
        var fixture = new Fixture();

        await Assert.ThrowsAsync<ValidationException>(
            () => fixture.Service.GenerateChallengeAsync(Guid.NewGuid(), new string('a', 65)));
    }

    [Fact]
    public async Task GenerateChallengeAsync_HandleOfExactly64_Accepted()
    {
        var handle = new string('a', 64);
        var fixture = new Fixture();

        var response = await fixture.Service.GenerateChallengeAsync(Guid.NewGuid(), handle);

        Assert.Equal(handle, response.Handle);
    }

    [Fact]
    public async Task GenerateChallengeAsync_TrimsHandleBeforePersistenceAndQueries()
    {
        var fixture = new Fixture();

        var response = await fixture.Service.GenerateChallengeAsync(Guid.NewGuid(), "  tourist  ");

        Assert.Equal("tourist", response.Handle);
        Assert.Equal("tourist", Assert.Single(fixture.Added).Handle);
        fixture.Accounts.Verify(r => r.GetVerifiedByPlatformAndHandleAsync(
            ExternalPlatform.Codeforces, "tourist", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateChallengeAsync_PreservesHandleCasing()
    {
        var fixture = new Fixture();

        var response = await fixture.Service.GenerateChallengeAsync(Guid.NewGuid(), "ToUrIsT");

        Assert.Equal("ToUrIsT", response.Handle);
        fixture.Accounts.Verify(r => r.GetVerifiedByPlatformAndHandleAsync(
            ExternalPlatform.Codeforces, "ToUrIsT", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateChallengeAsync_CreatesPendingAccountWithFifteenMinuteExpiry()
    {
        var fixture = new Fixture(new FixedTimeProvider(Now));
        var userId = Guid.NewGuid();

        var response = await fixture.Service.GenerateChallengeAsync(userId, "tourist");

        Assert.Equal((int)ExternalPlatform.Codeforces, response.Platform);
        Assert.Equal((int)AccountVerificationStatus.Pending, response.VerificationStatus);
        Assert.Equal(Now.AddMinutes(15), response.TokenExpiresAt);
        Assert.Null(response.VerifiedAt);
        Assert.Equal(userId, Assert.Single(fixture.Added).UserId);
    }

    [Fact]
    public async Task GenerateChallengeAsync_TokenHasPrefixAndSixteenEncodedBytes()
    {
        var fixture = new Fixture();

        var response = await fixture.Service.GenerateChallengeAsync(Guid.NewGuid(), "tourist");

        Assert.NotNull(response.VerificationToken);
        Assert.StartsWith(TokenPrefix, response.VerificationToken!);
        var randomPart = response.VerificationToken![TokenPrefix.Length..];
        Assert.Equal(32, randomPart.Length);
        Assert.Equal(16, Convert.FromHexString(randomPart).Length);
    }

    [Fact]
    public async Task GenerateChallengeAsync_ConsecutiveCallsProduceDifferentTokens()
    {
        var fixture = new Fixture();
        var userId = Guid.NewGuid();

        var first = await fixture.Service.GenerateChallengeAsync(userId, "tourist");
        var second = await fixture.Service.GenerateChallengeAsync(userId, "tourist");

        Assert.NotEqual(first.VerificationToken, second.VerificationToken);
    }

    [Fact]
    public async Task GenerateChallengeAsync_ThousandCallsProduceUniqueTokens()
    {
        var fixture = new Fixture();
        var userId = Guid.NewGuid();
        var tokens = new HashSet<string>();

        for (var i = 0; i < 1000; i++)
        {
            var response = await fixture.Service.GenerateChallengeAsync(userId, $"handle{i}");
            Assert.True(tokens.Add(response.VerificationToken!));
        }

        Assert.Equal(1000, tokens.Count);
    }

    [Fact]
    public async Task GenerateChallengeAsync_OtherUsersVerifiedHandle_ThrowsConflictWithoutWrites()
    {
        var fixture = new Fixture();
        var verified = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Platform = ExternalPlatform.Codeforces,
            Handle = "tourist",
            Status = AccountVerificationStatus.Verified
        };
        fixture.Accounts.Setup(r => r.GetVerifiedByPlatformAndHandleAsync(
                ExternalPlatform.Codeforces, "tourist", It.IsAny<CancellationToken>()))
            .ReturnsAsync(verified);

        await Assert.ThrowsAsync<ConflictException>(
            () => fixture.Service.GenerateChallengeAsync(Guid.NewGuid(), "tourist"));

        fixture.Accounts.Verify(r => r.AddAsync(It.IsAny<UserExternalAccount>(), It.IsAny<CancellationToken>()), Times.Never);
        fixture.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateChallengeAsync_UserWithVerifiedCodeforces_ThrowsConflictWithoutWrites()
    {
        var fixture = new Fixture();
        var userId = Guid.NewGuid();
        var verified = new UserExternalAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Platform = ExternalPlatform.Codeforces,
            Handle = "old_handle",
            Status = AccountVerificationStatus.Verified
        };
        fixture.Accounts.Setup(r => r.GetByUserAndPlatformAsync(
                userId, ExternalPlatform.Codeforces, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verified);

        await Assert.ThrowsAsync<ConflictException>(
            () => fixture.Service.GenerateChallengeAsync(userId, "new_handle"));

        fixture.Accounts.Verify(r => r.AddAsync(It.IsAny<UserExternalAccount>(), It.IsAny<CancellationToken>()), Times.Never);
        fixture.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateChallengeAsync_UnknownUser_ThrowsNotFoundBeforeQueries()
    {
        var fixture = new Fixture();
        var userId = Guid.NewGuid();
        fixture.Users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => fixture.Service.GenerateChallengeAsync(userId, "tourist"));

        fixture.Accounts.Verify(r => r.GetVerifiedByPlatformAndHandleAsync(
            It.IsAny<ExternalPlatform>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        fixture.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateChallengeAsync_CancelledToken_StopsBeforeWrites()
    {
        var fixture = new Fixture();
        var userId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        fixture.Users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .Returns((Guid _, CancellationToken ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return Task.FromResult<User?>(new User { Id = userId, Name = "Test", Email = "u@test.com", PasswordHash = "hash" });
            });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => fixture.Service.GenerateChallengeAsync(userId, "tourist", cts.Token));

        fixture.Accounts.Verify(r => r.AddAsync(It.IsAny<UserExternalAccount>(), It.IsAny<CancellationToken>()), Times.Never);
        fixture.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateChallengeAsync_ForwardsTokenToEveryAsyncCall()
    {
        var fixture = new Fixture();
        var userId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;

        await fixture.Service.GenerateChallengeAsync(userId, "tourist", ct);

        fixture.Users.Verify(r => r.GetByIdAsync(userId, ct), Times.Once);
        fixture.Accounts.Verify(r => r.GetVerifiedByPlatformAndHandleAsync(
            ExternalPlatform.Codeforces, "tourist", ct), Times.Once);
        fixture.Accounts.Verify(r => r.GetByUserAndPlatformAsync(
            userId, ExternalPlatform.Codeforces, ct), Times.Once);
        fixture.Accounts.Verify(r => r.AddAsync(It.IsAny<UserExternalAccount>(), ct), Times.Once);
        fixture.Uow.Verify(u => u.SaveChangesAsync(ct), Times.Once);
    }

    [Fact]
    public async Task GenerateChallengeAsync_SaveFailurePropagatesWithoutTranslation()
    {
        var fixture = new Fixture();
        fixture.Uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("save failed"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => fixture.Service.GenerateChallengeAsync(Guid.NewGuid(), "tourist"));

        Assert.Equal("save failed", exception.Message);
    }
}
