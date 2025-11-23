using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.ProductCatalog.DTOs;

/// <summary>
/// Represents a product row in CSV import/export.
/// Field order matches the CSV column order for template consistency.
/// </summary>
public class ProductCsvRow
{
    /// <summary>
    /// Stock Keeping Unit - unique product identifier within the store.
    /// Required for both import and update operations.
    /// </summary>
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Product title/name.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Product description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category name (will be matched to existing categories).
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Product price in USD.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Stock quantity available.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Product weight in grams (optional).
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Product length in centimeters (optional).
    /// </summary>
    public decimal? Length { get; set; }

    /// <summary>
    /// Product width in centimeters (optional).
    /// </summary>
    public decimal? Width { get; set; }

    /// <summary>
    /// Product height in centimeters (optional).
    /// </summary>
    public decimal? Height { get; set; }

    /// <summary>
    /// Pipe-separated list of image URLs (e.g., "url1|url2|url3").
    /// </summary>
    public string? ImageUrls { get; set; }

    /// <summary>
    /// Product status: Draft, Published, or Archived.
    /// Defaults to "Draft" if not specified.
    /// </summary>
    public string Status { get; set; } = "Draft";
}

/// <summary>
/// Result of a CSV import operation.
/// </summary>
public class ProductCsvImportResult
{
    /// <summary>
    /// Whether the import was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Number of products successfully created or updated.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of products that failed validation or import.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// List of validation errors with row numbers.
    /// </summary>
    public List<ProductCsvRowError> Errors { get; set; } = new();

    /// <summary>
    /// Overall message about the import result.
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Represents a validation error for a specific CSV row.
/// </summary>
public class ProductCsvRowError
{
    /// <summary>
    /// Row number in the CSV file (1-based, excluding header).
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// SKU of the product that failed (if available).
    /// </summary>
    public string? SKU { get; set; }

    /// <summary>
    /// List of validation error messages for this row.
    /// </summary>
    public List<string> ErrorMessages { get; set; } = new();
}

/// <summary>
/// Request for exporting products to CSV.
/// </summary>
public class ExportProductsCsvRequest
{
    /// <summary>
    /// Optional status filter (Draft, Published, Archived).
    /// If null, all products are exported.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Optional category filter.
    /// If provided, only products in this category are exported.
    /// </summary>
    public Guid? CategoryId { get; set; }
}
