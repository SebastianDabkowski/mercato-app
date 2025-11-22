using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Administration.Models;

/// <summary>
/// Represents an audit log entry for administrative actions.
/// All admin actions affecting users, sellers, and catalogs are logged here.
/// </summary>
public class AdminAuditLog
{
    /// <summary>
    /// Unique identifier for the audit log entry.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the administrator who performed the action.
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string AdminUserId { get; set; } = string.Empty;

    /// <summary>
    /// Email of the administrator for quick reference.
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>
    /// Type of action performed (e.g., UserActivated, UserDeactivated, StoreVerified, CategoryCreated).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Entity type that was affected (e.g., User, Store, Category).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the entity that was affected.
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of the action.
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// JSON representation of the changes made (before/after values).
    /// </summary>
    [MaxLength(4000)]
    public string? ChangesJson { get; set; }

    /// <summary>
    /// IP address from which the action was performed.
    /// </summary>
    [MaxLength(50)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Timestamp when the action was performed.
    /// </summary>
    [Required]
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Constants for audit log action types.
/// </summary>
public static class AuditActions
{
    // User actions
    public const string UserActivated = "UserActivated";
    public const string UserDeactivated = "UserDeactivated";
    public const string UserPasswordReset = "UserPasswordReset";
    public const string UserInviteSent = "UserInviteSent";
    public const string UserRoleChanged = "UserRoleChanged";

    // Store/Seller actions
    public const string StoreActivated = "StoreActivated";
    public const string StoreDeactivated = "StoreDeactivated";
    public const string StoreVerified = "StoreVerified";
    public const string StoreCommissionChanged = "StoreCommissionChanged";

    // Category actions
    public const string CategoryCreated = "CategoryCreated";
    public const string CategoryUpdated = "CategoryUpdated";
    public const string CategoryDeactivated = "CategoryDeactivated";
    public const string CategoryActivated = "CategoryActivated";
    public const string CategoryCommissionChanged = "CategoryCommissionChanged";
}

/// <summary>
/// Constants for entity types in audit logs.
/// </summary>
public static class EntityTypes
{
    public const string User = "User";
    public const string Store = "Store";
    public const string Category = "Category";
}
