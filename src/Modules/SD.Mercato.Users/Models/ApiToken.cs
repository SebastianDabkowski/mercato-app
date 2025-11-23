namespace SD.Mercato.Users.Models;

/// <summary>
/// Represents an API token for partner integrations.
/// Separate from user JWT tokens, used for external system authentication.
/// </summary>
public class ApiToken
{
    /// <summary>
    /// Unique identifier for the API token.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The actual token value (hashed for security).
    /// </summary>
    public required string TokenHash { get; set; }

    /// <summary>
    /// Display name for the token (e.g., "Baselinker Integration").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// ID of the user (seller) who owns this token.
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// Navigation property to the user who owns this token.
    /// </summary>
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Store ID this token is scoped to (if applicable).
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// Permissions granted to this token (comma-separated).
    /// E.g., "products:read,products:write,orders:read"
    /// </summary>
    public required string Permissions { get; set; }

    /// <summary>
    /// When the token was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the token expires (null = never expires).
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// When the token was last used.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Whether the token is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// IP address restrictions (comma-separated, null = no restrictions).
    /// </summary>
    public string? IpWhitelist { get; set; }

    /// <summary>
    /// Notes about this token (internal use).
    /// </summary>
    public string? Notes { get; set; }
}
