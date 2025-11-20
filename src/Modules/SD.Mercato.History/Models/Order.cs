using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.History.Models;

/// <summary>
/// Represents a marketplace-level order placed by a buyer.
/// Contains items from one or multiple sellers, grouped into SubOrders.
/// </summary>
public class Order
{
    /// <summary>
    /// Unique identifier for the order.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Unique human-readable order number (e.g., "MKT-2024-000123").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// User ID of the buyer (foreign key to ApplicationUser in Users module).
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Buyer's email address at the time of order.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string BuyerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Buyer's phone number for delivery contact.
    /// </summary>
    [Required]
    [Phone]
    [MaxLength(20)]
    public string BuyerPhone { get; set; } = string.Empty;

    /// <summary>
    /// Delivery address - recipient name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string DeliveryRecipientName { get; set; } = string.Empty;

    /// <summary>
    /// Delivery address - line 1 (street address).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string DeliveryAddressLine1 { get; set; } = string.Empty;

    /// <summary>
    /// Delivery address - line 2 (apartment, suite, etc.).
    /// </summary>
    [MaxLength(200)]
    public string? DeliveryAddressLine2 { get; set; }

    /// <summary>
    /// Delivery address - city.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string DeliveryCity { get; set; } = string.Empty;

    /// <summary>
    /// Delivery address - state or province.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string DeliveryState { get; set; } = string.Empty;

    /// <summary>
    /// Delivery address - postal code.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string DeliveryPostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Delivery address - country (default: "United States" for MVP).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string DeliveryCountry { get; set; } = "United States";

    /// <summary>
    /// Total amount paid by buyer (products + shipping + fees).
    /// </summary>
    [Required]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Currency code (default: "USD" for MVP).
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Payment status: Pending, Paid, Failed, Refunded.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PaymentStatus { get; set; } = OrderPaymentStatus.Pending;

    /// <summary>
    /// Payment method used (e.g., "Credit Card", "Debit Card").
    /// </summary>
    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Payment transaction ID from payment gateway.
    /// </summary>
    [MaxLength(200)]
    public string? PaymentTransactionId { get; set; }

    /// <summary>
    /// Overall order status: Pending, Processing, Completed, Cancelled.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = OrderStatus.Pending;

    /// <summary>
    /// Collection of sub-orders, one for each seller in the order.
    /// </summary>
    public List<SubOrder> SubOrders { get; set; } = new();

    /// <summary>
    /// Timestamp when the order was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the order was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Timestamp when payment was completed.
    /// </summary>
    public DateTime? PaidAt { get; set; }
}

/// <summary>
/// Order payment status constants.
/// </summary>
public static class OrderPaymentStatus
{
    public const string Pending = "Pending";
    public const string Paid = "Paid";
    public const string Failed = "Failed";
    public const string Refunded = "Refunded";
}

/// <summary>
/// Order status constants.
/// </summary>
public static class OrderStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
}
