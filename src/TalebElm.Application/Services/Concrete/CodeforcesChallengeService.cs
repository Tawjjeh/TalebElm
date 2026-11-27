using System.Security.Cryptography;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Application.Services.Concrete;

public sealed class CodeforcesChallengeService(
    IUnitOfWork unitOfWork,
    TimeProvider? timeProvider = null) : ICodeforcesChallengeService
{
    private const string TokenPrefix = "taleb-elm-verify-";
    private const int HandleMaxLength = 64;
    private const int TokenByteCount = 16;
    private static readonly TimeSpan ChallengeLifetime = TimeSpan.FromMinutes(15);

    public async Task<ExternalAccountResponse> GenerateChallengeAsync(Guid userId, string handle, CancellationToken ct = default)
    {
        var trimmedHandle = ValidateHandle(handle);

        var user = await unitOfWork.Users.GetByIdAsync(userId, ct);
        if (user is null)
            throw new NotFoundException($"User with id '{userId}' was not found.");

        var verifiedHandleOwner = await unitOfWork.UserExternalAccounts
            .GetVerifiedByPlatformAndHandleAsync(ExternalPlatform.Codeforces, trimmedHandle, ct);
        if (verifiedHandleOwner is not null)
            throw new ConflictException("This Codeforces handle is already verified.");

        var account = await unitOfWork.UserExternalAccounts
            .GetByUserAndPlatformAsync(userId, ExternalPlatform.Codeforces, ct);
        if (account is { Status: AccountVerificationStatus.Verified })
            throw new ConflictException("The user already has a verified Codeforces account.");

        var token = CreateToken();
        var expiresAtUtc = (timeProvider ?? TimeProvider.System).GetUtcNow().UtcDateTime.Add(ChallengeLifetime);

        if (account is null)
        {
            account = new UserExternalAccount
            {
                UserId = userId,
                Platform = ExternalPlatform.Codeforces
            };
            await unitOfWork.UserExternalAccounts.AddAsync(account, ct);
        }

        account.Handle = trimmedHandle;
        account.Status = AccountVerificationStatus.Pending;
        account.VerificationToken = token;
        account.TokenExpiresAtUtc = expiresAtUtc;

        await unitOfWork.SaveChangesAsync(ct);

        return new ExternalAccountResponse(
            account.Id,
            (int)account.Platform,
            account.Handle,
            (int)account.Status,
            token,
            ToDateTimeOffset(account.TokenExpiresAtUtc),
            ToDateTimeOffset(account.VerifiedAtUtc));
    }

    private static string ValidateHandle(string? handle)
    {
        if (string.IsNullOrWhiteSpace(handle))
            throw new ValidationException("A Codeforces handle is required.");

        var trimmed = handle.Trim();
        if (trimmed.Length > HandleMaxLength)
            throw new ValidationException($"A Codeforces handle must not exceed {HandleMaxLength} characters.");

        return trimmed;
    }

    private static string CreateToken()
    {
        Span<byte> randomBytes = stackalloc byte[TokenByteCount];
        RandomNumberGenerator.Fill(randomBytes);
        var token = TokenPrefix + Convert.ToHexString(randomBytes).ToLowerInvariant();
        randomBytes.Clear();
        return token;
    }

    private static DateTimeOffset? ToDateTimeOffset(DateTime? value)
        => value is { } utc ? new DateTimeOffset(utc, TimeSpan.Zero) : null;
}
