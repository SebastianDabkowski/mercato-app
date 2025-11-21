using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Payments.Models;

/// <summary>
/// Represents the payment breakdown for a specific SubOrder.
/// Tracks commission calculation and seller's net amount per SubOrder.
/// This is the bridge between payment transactions and individual seller orders.
/// </summary>
public class SubOrderPayment
{
    /// <summary>
    /// Unique identifier for the SubOrder payment record.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// SubOrder ID (foreign key to SubOrder in History module).
    /// </summary>
    [Required]
    public Guid SubOrderId { get; set; }

    /// <summary>
    /// Payment transaction ID (foreign key to PaymentTransaction).
    /// </summary>
    [Required]
    public Guid PaymentTransactionId { get; set; }

    /// <summary>
    /// Store ID receiving this portion of payment (foreign key to Store).
    /// </summary>
    [Required]
    public Guid StoreId { get; set; }

    /// <summary>
    /// SubOrder product subtotal (before shipping).
    /// This is the base amount on which commission is calculated.
    /// </summary>
    [Required]
    public decimal ProductTotal { get; set; }

    /// <summary>
    /// Shipping cost for this SubOrder.
    /// Commission is NOT applied to shipping in the MVP.
    /// </summary>
    [Required]
    public decimal ShippingCost { get; set; }

    /// <summary>
    /// Total SubOrder amount (ProductTotal + ShippingCost).
    /// </summary>
    [Required]
    public decimal SubOrderTotal { get; set; }

    /// <summary>
    /// Commission rate applied to this SubOrder (e.g., 0.15 for 15%).
    /// This rate is captured at the time of transaction for audit purposes.
    /// </summary>
    [Required]
    public decimal CommissionRate { get; set; }

    /// <summary>
    /// Commission amount deducted from this SubOrder.
    /// Calculated as: ProductTotal × CommissionRate
    /// </summary>
    [Required]
    public decimal CommissionAmount { get; set; }

    /// <summary>
    /// Processing fee portion allocated to this SubOrder.
    /// The total payment processing fee is distributed proportionally across SubOrders.
    /// Calculated as: (SubOrderTotal / OrderTotal) × TotalProcessingFee
    /// </summary>
    [Required]
    public decimal ProcessingFeeAllocated { get; set; }

    /// <summary>
    /// Net amount seller will receive for this SubOrder (after all deductions).
    /// Calculated as: SubOrderTotal - CommissionAmount - ProcessingFeeAllocated
    /// </summary>
    [Required]
    public decimal SellerNetAmount { get; set; }

    /// <summary>
    /// Currency code (default: "USD" for MVP).
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Payout status: Pending (in escrow), Released (available for payout), PaidOut (included in payout).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PayoutStatus { get; set; } = SubOrderPayoutStatus.Pending;

    /// <summary>
    /// Payout ID if this SubOrder payment has been included in a payout.
    /// Null if not yet paid out.
    /// </summary>
    public Guid? PayoutId { get; set; }

    /// <summary>
    /// Navigation property to Payout.
    /// </summary>
    public Payout? Payout { get; set; }

    /// <summary>
    /// Timestamp when this record was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the payout status was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// SubOrder payout status constants.
/// </summary>
public static class SubOrderPayoutStatus
{
    /// <summary>
    /// Payment is in escrow, awaiting delivery confirmation.
    /// </summary>
    public const string Pending = "Pending";

    /// <summary>
    /// Delivery confirmed, funds released from escrow and available for payout.
    /// </summary>
    public const string Released = "Released";

    /// <summary>
    /// Included in a completed payout to the seller.
    /// </summary>
    public const string PaidOut = "PaidOut";
}
