namespace TalebElm.Application.Services.Abstraction;

/// <summary>
/// One-way password hashing for registration and login. The raw password only
/// ever exists inside request DTOs; only the resulting hash reaches the entity.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);

    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}
