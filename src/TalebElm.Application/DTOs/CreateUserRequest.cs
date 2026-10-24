namespace TalebElm.Application.DTOs;

/// <summary>
/// Represents the data required to create a new user.
/// </summary>
/// <param name="Name">The name of the user.</param>
/// <param name="Email">The email address of the user.</param>
public record CreateUserRequest(string Name,string Email);