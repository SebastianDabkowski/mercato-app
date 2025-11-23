using SD.Mercato.ProductCatalog.DTOs;

namespace SD.Mercato.ProductCatalog.Services;

/// <summary>
/// Service for importing and exporting products in CSV format.
/// </summary>
public interface IProductCsvService
{
    /// <summary>
    /// Import products from CSV file.
    /// Creates new products or updates existing ones based on SKU.
    /// </summary>
    /// <param name="storeId">Store ID that owns these products.</param>
    /// <param name="csvStream">CSV file stream.</param>
    /// <returns>Import result with success/error details.</returns>
    Task<ProductCsvImportResult> ImportProductsAsync(Guid storeId, Stream csvStream);

    /// <summary>
    /// Export products to CSV format.
    /// </summary>
    /// <param name="storeId">Store ID to export products from.</param>
    /// <param name="request">Optional filters for the export.</param>
    /// <returns>CSV file content as byte array.</returns>
    Task<byte[]> ExportProductsAsync(Guid storeId, ExportProductsCsvRequest? request = null);

    /// <summary>
    /// Generate a CSV template file for product import.
    /// Returns an empty CSV with headers and optionally a sample row.
    /// </summary>
    /// <param name="includeSampleRow">Whether to include a sample data row.</param>
    /// <returns>CSV template content as byte array.</returns>
    byte[] GenerateTemplate(bool includeSampleRow = true);
}
