namespace TalebElm.Application.DTOs;

public record ExternalAccountResponse(
    Guid Id,
    int Platform,
    string Handle,
    int VerificationStatus,
    string? VerificationToken,
    DateTimeOffset? TokenExpiresAt,
    DateTimeOffset? VerifiedAt);
