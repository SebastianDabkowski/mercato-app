using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.History.Models;

/// <summary>
/// Represents a seller-specific portion of an order.
/// Each SubOrder contains items from a single seller.
/// </summary>
public class SubOrder
{
    /// <summary>
    /// Unique identifier for the sub-order.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Unique human-readable sub-order number (e.g., "SUB-2024-000456").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SubOrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Parent order ID.
    /// </summary>
    [Required]
    public Guid OrderId { get; set; }

    /// <summary>
    /// Navigation property to parent Order.
    /// </summary>
    public Order? Order { get; set; }

    /// <summary>
    /// Store ID that owns the products in this sub-order.
    /// </summary>
    [Required]
    public Guid StoreId { get; set; }

    /// <summary>
    /// Store name at the time of order (for audit/history).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// Subtotal for products in this sub-order (before shipping).
    /// </summary>
    [Required]
    public decimal ProductsTotal { get; set; }

    /// <summary>
    /// Shipping cost for this sub-order.
    /// </summary>
    [Required]
    public decimal ShippingCost { get; set; }

    /// <summary>
    /// Total amount for this sub-order (products + shipping).
    /// </summary>
    [Required]
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Shipping method selected for this sub-order.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ShippingMethod { get; set; } = string.Empty;

    /// <summary>
    /// Shipping tracking number (provided by seller after shipment).
    /// </summary>
    [MaxLength(200)]
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// Carrier name (e.g., "UPS", "FedEx", "USPS") - provided by seller when marking as shipped.
    /// </summary>
    [MaxLength(100)]
    public string? CarrierName { get; set; }

    /// <summary>
    /// Estimated delivery date (optional).
    /// </summary>
    public DateTime? EstimatedDeliveryDate { get; set; }

    /// <summary>
    /// Sub-order status: Pending, Processing, Shipped, Delivered, Cancelled.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = SubOrderStatus.Pending;

    /// <summary>
    /// Collection of items in this sub-order.
    /// </summary>
    public List<SubOrderItem> Items { get; set; } = new();

    /// <summary>
    /// Timestamp when the sub-order was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the sub-order was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Timestamp when the seller shipped the sub-order.
    /// </summary>
    public DateTime? ShippedAt { get; set; }

    /// <summary>
    /// Timestamp when the sub-order was delivered.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }
}

/// <summary>
/// Sub-order status constants.
/// </summary>
public static class SubOrderStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Shipped = "Shipped";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";
}

/// <summary>
/// Represents an individual product item in a sub-order.
/// </summary>
public class SubOrderItem
{
    /// <summary>
    /// Unique identifier for the sub-order item.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Sub-order ID this item belongs to.
    /// </summary>
    [Required]
    public Guid SubOrderId { get; set; }

    /// <summary>
    /// Navigation property to SubOrder.
    /// </summary>
    public SubOrder? SubOrder { get; set; }

    /// <summary>
    /// Product ID (foreign key to Product in ProductCatalog module).
    /// </summary>
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Product SKU at the time of order (for audit/history).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Product title at the time of order (for audit/history).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ProductTitle { get; set; } = string.Empty;

    /// <summary>
    /// Product image URL at the time of order (for audit/history).
    /// </summary>
    [MaxLength(500)]
    public string? ProductImageUrl { get; set; }

    /// <summary>
    /// Quantity ordered.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    /// <summary>
    /// Price per unit at the time of order.
    /// </summary>
    [Required]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Subtotal for this item (Quantity × UnitPrice).
    /// </summary>
    [Required]
    public decimal Subtotal { get; set; }
}
