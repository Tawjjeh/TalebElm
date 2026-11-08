namespace TalebElm.Application.DTOs
{
    /// <summary>
    /// Represents the response data for a user.
    /// </summary>
    /// <param name="Id">The unique identifier of the user.</param>
    /// <param name="Name">The name of the user.</param>
    /// <param name="Email">The email address of the user.</param>
    public record UserResponse(Guid Id,string Name,string Email);
}