using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace TalebElm.Api.Authentication;

/// <summary>
/// Placeholder authentication scheme registered until a real credential scheme
/// (JWT bearer) is implemented. It never authenticates a caller, so protected
/// endpoints answer 401 for anonymous requests and stay usable for authenticated
/// callers once a real scheme replaces it.
/// </summary>
public sealed class NoopAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "TalebElm";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        => Task.FromResult(AuthenticateResult.NoResult());
}
