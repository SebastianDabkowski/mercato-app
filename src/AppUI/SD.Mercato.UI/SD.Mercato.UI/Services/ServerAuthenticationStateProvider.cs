using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace SD.Mercato.UI.Services;

/// <summary>
/// Server-side authentication state provider for pre-rendering.
/// Returns an anonymous user during server-side pre-rendering.
/// The actual authentication state is provided by the client-side provider after WebAssembly loads.
/// </summary>
public class ServerAuthenticationStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        return Task.FromResult(new AuthenticationState(anonymous));
    }
}
