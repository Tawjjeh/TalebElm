using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TalebElm.Domain.Enums;

namespace TalebElm.Tests.IntegrationTests;

/// <summary>
/// Builds test authentication claims so controller tests can run as a signed-in
/// caller without starting the HTTP pipeline or a real identity provider.
/// </summary>
internal static class TestAuthenticationExtensions
{
    public static T AsUser<T>(this T controller, Guid userId, UserRole role = UserRole.Student)
        where T : ControllerBase
        => controller.WithPrincipal(new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role.ToString())
            ], "TestAuthentication")));

    public static T AsAnonymous<T>(this T controller) where T : ControllerBase
        => controller.WithPrincipal(new ClaimsPrincipal(new ClaimsIdentity()));

    private static T WithPrincipal<T>(this T controller, ClaimsPrincipal principal)
        where T : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        return controller;
    }
}
