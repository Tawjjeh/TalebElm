using Microsoft.AspNetCore.Identity;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Entities;

namespace TalebElm.Application.Services.Concrete;

/// <summary>
/// PBKDF2 password hashing via ASP.NET Core Identity (salted, slow, versioned
/// format, constant-time verification). Chosen over BCrypt so the stack gains
/// no new third-party dependency; the versioned hash format also allows future
/// re-hashing upgrades without breaking stored passwords.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private static readonly PasswordHasher<User> Inner = new();

    public string HashPassword(string password)
    {
        // The framework hasher ignores the user instance for the current
        // format version; a throwaway instance keeps our port user-agnostic.
        return Inner.HashPassword(new User(), password);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        return Inner.VerifyHashedPassword(new User(), hashedPassword, providedPassword)
            != PasswordVerificationResult.Failed;
    }
}
