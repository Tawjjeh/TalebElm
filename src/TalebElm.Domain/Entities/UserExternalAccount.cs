using TalebElm.Domain.Enums;

namespace TalebElm.Domain.Entities;

public class UserExternalAccount : BaseEntity
{
    public Guid UserId { get; set; }
    public ExternalPlatform Platform { get; set; }
    public string Handle { get; set; } = string.Empty;
    public string? ExternalNumericId { get; set; }
    public string? VerificationToken { get; set; }
    public DateTime? TokenExpiresAtUtc { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public AccountVerificationStatus Status { get; set; }
}
