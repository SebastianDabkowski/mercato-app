using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Users.DTOs;

/// <summary>
/// Request to create a new API token.
/// </summary>
public class CreateApiTokenRequest
{
    /// <summary>
    /// Display name for the token.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    /// <summary>
    /// Permissions for this token (e.g., ["products:read", "products:write", "orders:read"]).
    /// </summary>
    [Required]
    public required List<string> Permissions { get; set; }

    /// <summary>
    /// Optional expiration date for the token.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Optional IP whitelist (comma-separated).
    /// </summary>
    [MaxLength(500)]
    public string? IpWhitelist { get; set; }

    /// <summary>
    /// Optional notes about this token.
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// Response when creating an API token.
/// </summary>
public class CreateApiTokenResponse
{
    /// <summary>
    /// Token ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The actual token value (only shown once at creation).
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// Display name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Permissions.
    /// </summary>
    public required List<string> Permissions { get; set; }

    /// <summary>
    /// When the token was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the token expires.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// DTO for API token information (without the actual token).
/// </summary>
public class ApiTokenDto
{
    /// <summary>
    /// Token ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Display name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Permissions.
    /// </summary>
    public required List<string> Permissions { get; set; }

    /// <summary>
    /// Store ID if scoped to a specific store.
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// When the token was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the token expires.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// When the token was last used.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Whether the token is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// IP whitelist.
    /// </summary>
    public string? IpWhitelist { get; set; }

    /// <summary>
    /// Masked preview of the token (e.g., "mrc_abc...xyz").
    /// </summary>
    public string? TokenPreview { get; set; }
}

/// <summary>
/// Request to update an API token.
/// </summary>
public class UpdateApiTokenRequest
{
    /// <summary>
    /// New display name (optional).
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; set; }

    /// <summary>
    /// Whether the token is active.
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Optional notes.
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}
