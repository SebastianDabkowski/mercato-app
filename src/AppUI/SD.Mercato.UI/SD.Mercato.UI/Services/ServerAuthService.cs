using SD.Mercato.UI.Client.Models;
using SD.Mercato.UI.Client.Services;

namespace SD.Mercato.UI.Services;

/// <summary>
/// Server-side implementation of IAuthService for pre-rendering.
/// This implementation returns empty/unauthenticated state since server doesn't have access to LocalStorage.
/// </summary>
public class ServerAuthService : IAuthService
{
    public Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        return Task.FromResult(new AuthResponse
        {
            Success = false,
            Message = "Registration must be performed on the client side"
        });
    }

    public Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        return Task.FromResult(new AuthResponse
        {
            Success = false,
            Message = "Login must be performed on the client side"
        });
    }

    public Task LogoutAsync()
    {
        return Task.CompletedTask;
    }

    public Task<UserDto?> GetCurrentUserAsync()
    {
        return Task.FromResult<UserDto?>(null);
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(false);
    }

    public Task<string?> GetTokenAsync()
    {
        return Task.FromResult<string?>(null);
    }
}
