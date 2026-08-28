namespace TalebElm.Application.DTOs
{
    public record UserResponse(
        Guid Id,
        string Name,
        string Email);
}
