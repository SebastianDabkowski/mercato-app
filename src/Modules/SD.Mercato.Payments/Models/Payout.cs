using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Payments.Models;

/// <summary>
/// Represents a payout from the platform to a seller.
/// Created when the seller's available balance is transferred to their bank account.
/// </summary>
public class Payout
{
    /// <summary>
    /// Unique identifier for the payout.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Store ID receiving the payout (foreign key to Store in SellerPanel module).
    /// </summary>
    [Required]
    public Guid StoreId { get; set; }

    /// <summary>
    /// Net amount paid to seller (after commission and fees).
    /// </summary>
    [Required]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gross amount before deductions (sum of all SubOrder totals included).
    /// </summary>
    [Required]
    public decimal GrossAmount { get; set; }

    /// <summary>
    /// Total commission amount deducted (sum of all commissions from included orders).
    /// </summary>
    [Required]
    public decimal CommissionAmount { get; set; }

    /// <summary>
    /// Total processing fees deducted (sum of all processing fees from included orders).
    /// </summary>
    [Required]
    public decimal ProcessingFeeAmount { get; set; }

    /// <summary>
    /// Currency code (default: "USD" for MVP).
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Payout status: Pending, Processing, Completed, Failed.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = PayoutStatus.Pending;

    /// <summary>
    /// Payout method (e.g., "BankTransfer", "PayPal").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PayoutMethod { get; set; } = "BankTransfer";

    /// <summary>
    /// External payout transaction ID from payment gateway or banking system.
    /// </summary>
    [MaxLength(200)]
    public string? ExternalTransactionId { get; set; }

    /// <summary>
    /// SubOrder IDs included in this payout (stored as comma-separated GUIDs).
    /// </summary>
    [MaxLength(4000)]
    public string? SubOrderIds { get; set; }

    /// <summary>
    /// Error message if payout failed.
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp when the payout was scheduled.
    /// </summary>
    [Required]
    public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the payout was completed or failed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Timestamp when the payout was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Payout status constants.
/// </summary>
public static class PayoutStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
}
