using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.History.DTOs;

/// <summary>
/// Represents an order line item in CSV export for seller accounting and integration.
/// Each row represents one product line in a SubOrder.
/// </summary>
public class OrderCsvRow
{
    /// <summary>
    /// Marketplace-level order number.
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Seller-specific sub-order number.
    /// </summary>
    public string SubOrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Date when the order was created.
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// Current status of the sub-order.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Buyer's email address.
    /// </summary>
    public string BuyerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Buyer's phone number.
    /// </summary>
    public string BuyerPhone { get; set; } = string.Empty;

    /// <summary>
    /// Delivery recipient name.
    /// </summary>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    /// Full delivery address (formatted).
    /// </summary>
    public string DeliveryAddress { get; set; } = string.Empty;

    /// <summary>
    /// Product SKU.
    /// </summary>
    public string ProductSKU { get; set; } = string.Empty;

    /// <summary>
    /// Product title/name.
    /// </summary>
    public string ProductTitle { get; set; } = string.Empty;

    /// <summary>
    /// Quantity ordered.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Price per unit.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Line subtotal (Quantity × UnitPrice).
    /// </summary>
    public decimal LineSubtotal { get; set; }

    /// <summary>
    /// Shipping cost for this sub-order (shown on first line only).
    /// </summary>
    public decimal ShippingCost { get; set; }

    /// <summary>
    /// Total amount for the sub-order (products + shipping).
    /// </summary>
    public decimal SubOrderTotal { get; set; }

    /// <summary>
    /// Platform commission amount (15% of product total, not shipping).
    /// </summary>
    public decimal PlatformCommission { get; set; }

    /// <summary>
    /// Payment processing fee (2.9% + $0.30 on total).
    /// </summary>
    public decimal ProcessingFee { get; set; }

    /// <summary>
    /// Net amount seller receives (after commission and fees).
    /// </summary>
    public decimal NetAmount { get; set; }

    /// <summary>
    /// Shipping method selected.
    /// </summary>
    public string ShippingMethod { get; set; } = string.Empty;

    /// <summary>
    /// Tracking number (if shipped).
    /// </summary>
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// Date when shipped (if applicable).
    /// </summary>
    public DateTime? ShippedDate { get; set; }

    /// <summary>
    /// Date when delivered (if applicable).
    /// </summary>
    public DateTime? DeliveredDate { get; set; }
}

/// <summary>
/// Request for exporting orders to CSV.
/// </summary>
public class ExportOrdersCsvRequest
{
    /// <summary>
    /// Start date for order filtering (inclusive).
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// End date for order filtering (inclusive).
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Optional status filter (Pending, Processing, Shipped, Delivered, Cancelled).
    /// If null, all statuses are included.
    /// </summary>
    public string? Status { get; set; }
}

/// <summary>
/// Result of a CSV export operation.
/// </summary>
public class OrderCsvExportResult
{
    /// <summary>
    /// Whether the export was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Number of order line items exported.
    /// </summary>
    public int RecordCount { get; set; }

    /// <summary>
    /// CSV file content as byte array.
    /// </summary>
    public byte[]? FileContent { get; set; }

    /// <summary>
    /// Suggested filename for the export.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Error message if export failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
