using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.History.Models;

/// <summary>
/// Represents a customer service case for returns or complaints.
/// Linked to a SubOrder for proper seller routing.
/// </summary>
public class Case
{
    /// <summary>
    /// Unique identifier for the case.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Unique human-readable case number (e.g., "CASE-2024-000123").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string CaseNumber { get; set; } = string.Empty;

    /// <summary>
    /// Type of case: Return or Complaint.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string CaseType { get; set; } = CaseTypes.Complaint;

    /// <summary>
    /// Current status of the case.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = CaseStatuses.New;

    /// <summary>
    /// User ID of the buyer who created this case.
    /// </summary>
    [Required]
    public string BuyerId { get; set; } = string.Empty;

    /// <summary>
    /// Order ID this case relates to.
    /// </summary>
    [Required]
    public Guid OrderId { get; set; }

    /// <summary>
    /// Navigation property to the Order.
    /// </summary>
    public Order? Order { get; set; }

    /// <summary>
    /// SubOrder ID this case relates to (seller-specific).
    /// </summary>
    [Required]
    public Guid SubOrderId { get; set; }

    /// <summary>
    /// Navigation property to the SubOrder.
    /// </summary>
    public SubOrder? SubOrder { get; set; }

    /// <summary>
    /// Store ID of the seller responsible for this case.
    /// </summary>
    [Required]
    public Guid StoreId { get; set; }

    /// <summary>
    /// Optional: Specific SubOrderItem ID if case relates to a single item.
    /// NULL means case relates to entire SubOrder.
    /// </summary>
    public Guid? SubOrderItemId { get; set; }

    /// <summary>
    /// Navigation property to the specific SubOrderItem (if applicable).
    /// </summary>
    public SubOrderItem? SubOrderItem { get; set; }

    /// <summary>
    /// Reason/category for the case.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description/message from the buyer.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Final outcome/resolution of the case.
    /// </summary>
    [MaxLength(1000)]
    public string? Resolution { get; set; }

    /// <summary>
    /// Collection of messages/responses in this case.
    /// </summary>
    public List<CaseMessage> Messages { get; set; } = new();

    /// <summary>
    /// Timestamp when the case was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the case was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Timestamp when the case was resolved/closed.
    /// </summary>
    public DateTime? ResolvedAt { get; set; }
}

/// <summary>
/// Case type constants.
/// </summary>
public static class CaseTypes
{
    public const string Return = "Return";
    public const string Complaint = "Complaint";
}

/// <summary>
/// Case status constants.
/// </summary>
public static class CaseStatuses
{
    public const string New = "New";
    public const string InReview = "In Review";
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
    public const string Resolved = "Resolved";
}
