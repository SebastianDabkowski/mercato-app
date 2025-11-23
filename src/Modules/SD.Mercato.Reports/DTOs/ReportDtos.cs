namespace SD.Mercato.Reports.DTOs;

/// <summary>
/// Request for seller financial report for a specific period.
/// </summary>
public record SellerFinancialReportRequest
{
    /// <summary>
    /// Store ID to generate report for.
    /// </summary>
    public required Guid StoreId { get; init; }

    /// <summary>
    /// Period start date (inclusive).
    /// </summary>
    public required DateTime StartDate { get; init; }

    /// <summary>
    /// Period end date (inclusive).
    /// </summary>
    public required DateTime EndDate { get; init; }
}

/// <summary>
/// Seller financial summary for a specific period.
/// Displays GMV, commissions, and net amount.
/// </summary>
public record SellerFinancialSummary
{
    /// <summary>
    /// Store ID.
    /// </summary>
    public required Guid StoreId { get; init; }

    /// <summary>
    /// Store name.
    /// </summary>
    public required string StoreName { get; init; }

    /// <summary>
    /// Period start date.
    /// </summary>
    public required DateTime PeriodStartDate { get; init; }

    /// <summary>
    /// Period end date.
    /// </summary>
    public required DateTime PeriodEndDate { get; init; }

    /// <summary>
    /// Total Gross Merchandise Value (GMV).
    /// Sum of all SubOrder totals (products + shipping).
    /// </summary>
    public decimal TotalGMV { get; init; }

    /// <summary>
    /// Total product value (without shipping).
    /// </summary>
    public decimal TotalProductValue { get; init; }

    /// <summary>
    /// Total shipping fees collected.
    /// </summary>
    public decimal TotalShippingFees { get; init; }

    /// <summary>
    /// Total commission charged.
    /// </summary>
    public decimal TotalCommission { get; init; }

    /// <summary>
    /// Total processing fees allocated.
    /// </summary>
    public decimal TotalProcessingFees { get; init; }

    /// <summary>
    /// Net amount due to seller.
    /// </summary>
    public decimal NetAmountDue { get; init; }

    /// <summary>
    /// Number of completed orders in the period.
    /// </summary>
    public int OrderCount { get; init; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public string Currency { get; init; } = "USD";
}

/// <summary>
/// Detailed line item for commission breakdown.
/// </summary>
public record CommissionLineItem
{
    /// <summary>
    /// SubOrder ID.
    /// </summary>
    public required Guid SubOrderId { get; init; }

    /// <summary>
    /// SubOrder number.
    /// </summary>
    public required string SubOrderNumber { get; init; }

    /// <summary>
    /// Order date.
    /// </summary>
    public required DateTime OrderDate { get; init; }

    /// <summary>
    /// Product SKU.
    /// </summary>
    public required string ProductSku { get; init; }

    /// <summary>
    /// Product title.
    /// </summary>
    public required string ProductTitle { get; init; }

    /// <summary>
    /// Quantity ordered.
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// Unit price.
    /// </summary>
    public decimal UnitPrice { get; init; }

    /// <summary>
    /// Line item subtotal (Quantity × UnitPrice).
    /// </summary>
    public decimal Subtotal { get; init; }

    /// <summary>
    /// Commission rate applied.
    /// </summary>
    public decimal CommissionRate { get; init; }

    /// <summary>
    /// Commission amount for this line item.
    /// </summary>
    public decimal CommissionAmount { get; init; }
}

/// <summary>
/// Request to generate a monthly invoice for a seller.
/// </summary>
public record GenerateInvoiceRequest
{
    /// <summary>
    /// Store ID to generate invoice for.
    /// </summary>
    public required Guid StoreId { get; init; }

    /// <summary>
    /// Period start date (inclusive).
    /// </summary>
    public required DateTime PeriodStartDate { get; init; }

    /// <summary>
    /// Period end date (inclusive).
    /// </summary>
    public required DateTime PeriodEndDate { get; init; }

    /// <summary>
    /// User ID generating the invoice (for audit).
    /// </summary>
    public string? GeneratedBy { get; init; }
}

/// <summary>
/// Response after generating an invoice.
/// </summary>
public record GenerateInvoiceResponse
{
    /// <summary>
    /// Indicates if invoice generation was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Invoice ID if successful.
    /// </summary>
    public Guid? InvoiceId { get; init; }

    /// <summary>
    /// Invoice number if successful.
    /// </summary>
    public string? InvoiceNumber { get; init; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Global commission configuration DTO.
/// </summary>
public record GlobalCommissionConfigDto
{
    /// <summary>
    /// Configuration ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Default commission rate.
    /// </summary>
    public decimal DefaultCommissionRate { get; init; }

    /// <summary>
    /// Notes about the configuration.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Last modified by user ID.
    /// </summary>
    public string? LastModifiedBy { get; init; }

    /// <summary>
    /// Last updated timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Request to update global commission configuration.
/// </summary>
public record UpdateCommissionConfigRequest
{
    /// <summary>
    /// New default commission rate.
    /// </summary>
    public required decimal DefaultCommissionRate { get; init; }

    /// <summary>
    /// Notes about the change.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// User ID making the change.
    /// </summary>
    public required string ModifiedBy { get; init; }
}
