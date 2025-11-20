using SD.Mercato.Users.DTOs;

namespace SD.Mercato.Users.Services;

/// <summary>
/// Interface for authentication services.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user with email and password.
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Authenticates a user with an external provider (Google, Facebook).
    /// </summary>
    Task<AuthResponse> ExternalLoginAsync(ExternalLoginRequest request);

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    Task LogoutAsync(string userId);

    /// <summary>
    /// Gets user profile information.
    /// </summary>
    Task<UserDto?> GetUserProfileAsync(string userId);

    /// <summary>
    /// Updates user profile information.
    /// </summary>
    Task<bool> UpdateProfileAsync(string userId, UpdateProfileRequest request);
}
