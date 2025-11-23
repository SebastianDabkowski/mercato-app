using SD.Mercato.History.DTOs;

namespace SD.Mercato.History.Services;

/// <summary>
/// Service for exporting orders in CSV format.
/// </summary>
public interface IOrderCsvService
{
    /// <summary>
    /// Export orders for a seller's store to CSV format.
    /// Each row represents one product line item in a SubOrder.
    /// Includes all details needed for accounting and invoicing.
    /// </summary>
    /// <param name="storeId">Store ID to export orders from.</param>
    /// <param name="request">Date range and status filters.</param>
    /// <returns>Export result with CSV file content.</returns>
    Task<OrderCsvExportResult> ExportOrdersAsync(Guid storeId, ExportOrdersCsvRequest request);
}
