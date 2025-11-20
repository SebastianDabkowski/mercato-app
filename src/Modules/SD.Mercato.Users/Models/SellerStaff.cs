namespace SD.Mercato.Users.Models;

/// <summary>
/// Represents a staff member associated with a seller's store.
/// Future-proofs the system for multi-user seller accounts.
/// </summary>
public class SellerStaff
{
    /// <summary>
    /// Unique identifier for the seller staff record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the User (staff member).
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property to the User.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Foreign key to the Store (from SellerPanel module).
    /// </summary>
    public Guid StoreId { get; set; }

    /// <summary>
    /// Indicates if this is the primary owner of the store.
    /// </summary>
    public bool IsOwner { get; set; }

    /// <summary>
    /// Job title or role within the store (e.g., "Manager", "Sales Representative").
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Indicates whether this staff member is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp of when the staff member was added.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp of when the staff member was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // TODO: Future enhancement - add permissions/access control fields
    // For example: CanManageProducts, CanProcessOrders, CanViewReports, etc.
    // This will allow granular permissions for staff accounts when the UI is built later.
}
