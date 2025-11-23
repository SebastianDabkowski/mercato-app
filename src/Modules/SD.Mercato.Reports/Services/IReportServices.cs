using SD.Mercato.Reports.DTOs;
using SD.Mercato.Reports.Models;

namespace SD.Mercato.Reports.Services;

/// <summary>
/// Service for generating financial reports for sellers.
/// </summary>
public interface ISellerReportService
{
    /// <summary>
    /// Gets financial summary for a seller for a specific period.
    /// </summary>
    Task<SellerFinancialSummary?> GetFinancialSummaryAsync(SellerFinancialReportRequest request);

    /// <summary>
    /// Gets detailed commission breakdown for a seller for a specific period.
    /// </summary>
    Task<List<CommissionLineItem>> GetCommissionBreakdownAsync(SellerFinancialReportRequest request);
}

/// <summary>
/// Service for generating and managing seller invoices.
/// </summary>
public interface IInvoiceService
{
    /// <summary>
    /// Generates a monthly invoice for a seller.
    /// </summary>
    Task<GenerateInvoiceResponse> GenerateInvoiceAsync(GenerateInvoiceRequest request);

    /// <summary>
    /// Gets invoice HTML content for download.
    /// </summary>
    Task<string?> GetInvoiceHtmlAsync(Guid invoiceId);

    /// <summary>
    /// Gets invoice by ID.
    /// </summary>
    Task<SellerInvoice?> GetInvoiceByIdAsync(Guid invoiceId);

    /// <summary>
    /// Gets all invoices for a store.
    /// </summary>
    Task<List<SellerInvoice>> GetStoreInvoicesAsync(Guid storeId);
}

/// <summary>
/// Service for managing global commission configuration.
/// </summary>
public interface ICommissionConfigService
{
    /// <summary>
    /// Gets the active global commission configuration.
    /// </summary>
    Task<GlobalCommissionConfigDto?> GetActiveConfigAsync();

    /// <summary>
    /// Updates the global commission configuration.
    /// </summary>
    Task<GlobalCommissionConfigDto> UpdateConfigAsync(UpdateCommissionConfigRequest request);
}
