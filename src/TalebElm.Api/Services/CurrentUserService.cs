using System.Security.Claims;

namespace TalebElm.Api.Services;

public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public string? UserId()
        => accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
