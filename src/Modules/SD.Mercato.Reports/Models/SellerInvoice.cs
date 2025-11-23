using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Reports.Models;

/// <summary>
/// Represents a monthly commission invoice/statement for a seller.
/// Tracks all generated invoices for audit and historical purposes.
/// </summary>
public class SellerInvoice
{
    /// <summary>
    /// Unique identifier for the invoice.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Unique invoice number (e.g., "INV-2024-11-STORE123").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Store ID this invoice is for (foreign key to Store in SellerPanel module).
    /// </summary>
    [Required]
    public Guid StoreId { get; set; }

    /// <summary>
    /// Store name at the time of invoice generation (for audit/history).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// Invoice period start date (inclusive).
    /// </summary>
    [Required]
    public DateTime PeriodStartDate { get; set; }

    /// <summary>
    /// Invoice period end date (inclusive).
    /// </summary>
    [Required]
    public DateTime PeriodEndDate { get; set; }

    /// <summary>
    /// Total Gross Merchandise Value (GMV) for the period.
    /// Sum of all completed SubOrder totals (products + shipping).
    /// </summary>
    [Required]
    public decimal TotalGMV { get; set; }

    /// <summary>
    /// Total product value (without shipping).
    /// </summary>
    [Required]
    public decimal TotalProductValue { get; set; }

    /// <summary>
    /// Total shipping fees collected.
    /// </summary>
    [Required]
    public decimal TotalShippingFees { get; set; }

    /// <summary>
    /// Total commission charged for the period.
    /// </summary>
    [Required]
    public decimal TotalCommission { get; set; }

    /// <summary>
    /// Total processing fees allocated for the period.
    /// </summary>
    [Required]
    public decimal TotalProcessingFees { get; set; }

    /// <summary>
    /// Net amount due to seller (GMV - Commission - Processing Fees).
    /// </summary>
    [Required]
    public decimal NetAmountDue { get; set; }

    /// <summary>
    /// Number of orders included in this invoice.
    /// </summary>
    [Required]
    public int OrderCount { get; set; }

    /// <summary>
    /// Currency code (default: "USD" for MVP).
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Invoice status: Draft, Generated, Sent, Paid.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = InvoiceStatus.Draft;

    /// <summary>
    /// Generated HTML content of the invoice (for download/email).
    /// </summary>
    public string? HtmlContent { get; set; }

    /// <summary>
    /// Timestamp when the invoice was generated.
    /// </summary>
    [Required]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User ID of who generated the invoice (seller or admin).
    /// </summary>
    [MaxLength(450)]
    public string? GeneratedBy { get; set; }

    /// <summary>
    /// Timestamp when the invoice was sent to the seller.
    /// </summary>
    public DateTime? SentAt { get; set; }
}

/// <summary>
/// Invoice status constants.
/// </summary>
public static class InvoiceStatus
{
    public const string Draft = "Draft";
    public const string Generated = "Generated";
    public const string Sent = "Sent";
    public const string Paid = "Paid";
}
