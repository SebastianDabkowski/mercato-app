using SD.Mercato.Users.DTOs;

namespace SD.Mercato.Users.Services;

/// <summary>
/// Interface for API token management service.
/// </summary>
public interface IApiTokenService
{
    /// <summary>
    /// Create a new API token for a user.
    /// </summary>
    Task<CreateApiTokenResponse> CreateTokenAsync(string userId, Guid? storeId, CreateApiTokenRequest request);

    /// <summary>
    /// Validate an API token and return token ID, user ID and permissions if valid.
    /// </summary>
    Task<(bool IsValid, Guid? TokenId, string? UserId, Guid? StoreId, List<string>? Permissions)> ValidateTokenAsync(string token, string? ipAddress = null);

    /// <summary>
    /// Get all tokens for a user.
    /// </summary>
    Task<List<ApiTokenDto>> GetUserTokensAsync(string userId);

    /// <summary>
    /// Get a specific token by ID (for the owner user only).
    /// </summary>
    Task<ApiTokenDto?> GetTokenByIdAsync(Guid tokenId, string userId);

    /// <summary>
    /// Update a token (name, active status, notes).
    /// </summary>
    Task<bool> UpdateTokenAsync(Guid tokenId, string userId, UpdateApiTokenRequest request);

    /// <summary>
    /// Delete (revoke) a token.
    /// </summary>
    Task<bool> DeleteTokenAsync(Guid tokenId, string userId);

    /// <summary>
    /// Record token usage (update LastUsedAt).
    /// </summary>
    Task RecordTokenUsageAsync(Guid tokenId);
}
