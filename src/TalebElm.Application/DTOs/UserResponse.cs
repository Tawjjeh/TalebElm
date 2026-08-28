namespace TalebElm.Application.DTOs
{
    public record UserResponse(
        Guid id,
        string Name,
        string Email);
}
