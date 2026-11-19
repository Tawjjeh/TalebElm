using System.ComponentModel.DataAnnotations;

namespace TalebElm.Infrastructure.Authentication;

public class JwtOptions
{
    [Required, MinLength(32)]
    public const string SectionName = "Jwt";
    [Required]

    public string Key { get; init; } = string.Empty;
    [Required]

    public string Issuer { get; init; } = string.Empty;
    [Required]

    public string Audience { get; init; } = string.Empty;
    [Range(0, 60)]

    public int AccessTokenMinutes { get; init; }
    [Range(0, 120)]

    public int ClockSkewSeconds { get; init; }
}
