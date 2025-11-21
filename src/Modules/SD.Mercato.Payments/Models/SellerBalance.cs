using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Payments.Models;

/// <summary>
/// Represents the seller's financial balance in the marketplace.
/// Tracks pending and available funds from completed orders.
/// </summary>
public class SellerBalance
{
    /// <summary>
    /// Unique identifier for the balance record.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Store ID (foreign key to Store in SellerPanel module).
    /// </summary>
    [Required]
    public Guid StoreId { get; set; }

    /// <summary>
    /// Total pending amount (from orders not yet delivered).
    /// This amount is in escrow and not yet available for payout.
    /// </summary>
    [Required]
    public decimal PendingAmount { get; set; }

    /// <summary>
    /// Total available amount ready for payout (from delivered orders).
    /// This amount has cleared escrow and can be paid out to the seller.
    /// </summary>
    [Required]
    public decimal AvailableAmount { get; set; }

    /// <summary>
    /// Total amount already paid out to the seller.
    /// This is a running total for reporting and reconciliation purposes.
    /// </summary>
    [Required]
    public decimal TotalPaidOut { get; set; }

    /// <summary>
    /// Currency code (default: "USD" for MVP).
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Timestamp when the balance was last updated.
    /// </summary>
    [Required]
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the balance record was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
