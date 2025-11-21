using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Payments.Models;

/// <summary>
/// Represents a payment transaction in the marketplace.
/// Records all payment attempts and their outcomes for audit and reconciliation.
/// </summary>
public class PaymentTransaction
{
    /// <summary>
    /// Unique identifier for the transaction.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Order ID this payment is for (foreign key to Order in History module).
    /// </summary>
    [Required]
    public Guid OrderId { get; set; }

    /// <summary>
    /// Payment amount in the base currency.
    /// This is the total amount charged to the buyer.
    /// </summary>
    [Required]
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code (default: "USD" for MVP).
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Payment method used (e.g., "CreditCard", "DebitCard", "BankTransfer", "BLIK").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// External payment gateway transaction ID.
    /// This is the reference ID from the payment provider (e.g., Stripe, PayU).
    /// </summary>
    [MaxLength(200)]
    public string? PaymentGatewayTransactionId { get; set; }

    /// <summary>
    /// Payment gateway session ID (for checkout flow tracking).
    /// </summary>
    [MaxLength(200)]
    public string? PaymentGatewaySessionId { get; set; }

    /// <summary>
    /// Payment status: Pending, Completed, Failed, Refunded, Cancelled.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = PaymentTransactionStatus.Pending;

    /// <summary>
    /// Payment processing fee charged by the gateway.
    /// Typically calculated as: (Amount × gateway_rate) + fixed_fee
    /// Example: (Amount × 0.029) + 0.30
    /// </summary>
    [Required]
    public decimal ProcessingFee { get; set; }

    /// <summary>
    /// Error message if payment failed.
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Error code from payment gateway (if applicable).
    /// </summary>
    [MaxLength(100)]
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Timestamp when the transaction was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the payment was completed or failed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Additional metadata from payment gateway (stored as JSON).
    /// Can contain gateway-specific data like card last 4 digits, bank name, etc.
    /// </summary>
    [MaxLength(2000)]
    public string? Metadata { get; set; }
}

/// <summary>
/// Payment transaction status constants.
/// </summary>
public static class PaymentTransactionStatus
{
    public const string Pending = "Pending";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Refunded = "Refunded";
    public const string Cancelled = "Cancelled";
}
