using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SD.Mercato.ProductCatalog.Data;
using SD.Mercato.ProductCatalog.DTOs;
using SD.Mercato.ProductCatalog.Models;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace SD.Mercato.ProductCatalog.Services;

/// <summary>
/// Service for importing and exporting products in CSV format.
/// </summary>
public class ProductCsvService : IProductCsvService
{
    private readonly ProductCatalogDbContext _context;
    private readonly ILogger<ProductCsvService> _logger;

    public ProductCsvService(
        ProductCatalogDbContext context,
        ILogger<ProductCsvService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProductCsvImportResult> ImportProductsAsync(Guid storeId, Stream csvStream)
    {
        var result = new ProductCsvImportResult();
        var rowNumber = 1; // Start at 1 (excluding header)

        try
        {
            using var reader = new StreamReader(csvStream);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                TrimOptions = TrimOptions.Trim
            };

            using var csv = new CsvReader(reader, config);

            // Read all categories for lookup
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToDictionaryAsync(c => c.Name.ToLower(), c => c);

            // Read existing products for SKU checking
            var existingProducts = await _context.Products
                .Where(p => p.StoreId == storeId)
                .ToDictionaryAsync(p => p.SKU, p => p);

            // Process records in batches to avoid loading all into memory at once
            var records = csv.GetRecords<ProductCsvRow>();
            const int batchSize = 100;
            var recordsInBatch = 0;

            foreach (var row in records)
            {
                var rowErrors = new List<string>();

                try
                {
                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(row.SKU))
                    {
                        rowErrors.Add("SKU is required");
                    }

                    if (string.IsNullOrWhiteSpace(row.Title))
                    {
                        rowErrors.Add("Title is required");
                    }

                    if (string.IsNullOrWhiteSpace(row.Description))
                    {
                        rowErrors.Add("Description is required");
                    }

                    if (string.IsNullOrWhiteSpace(row.Category))
                    {
                        rowErrors.Add("Category is required");
                    }

                    // Validate string field length constraints
                    if (row.SKU != null && row.SKU.Length > 100)
                    {
                        rowErrors.Add("SKU exceeds maximum length of 100 characters");
                    }

                    if (row.Title != null && row.Title.Length > 200)
                    {
                        rowErrors.Add("Title exceeds maximum length of 200 characters");
                    }

                    if (row.Description != null && row.Description.Length > 5000)
                    {
                        rowErrors.Add("Description exceeds maximum length of 5000 characters");
                    }

                    // Validate price
                    if (row.Price <= 0)
                    {
                        rowErrors.Add("Price must be greater than 0");
                    }

                    // Validate stock quantity
                    if (row.StockQuantity < 0)
                    {
                        rowErrors.Add("Stock quantity cannot be negative");
                    }

                    // Validate dimensions if provided
                    if (row.Weight.HasValue && row.Weight.Value < 0)
                    {
                        rowErrors.Add("Weight cannot be negative");
                    }

                    if (row.Length.HasValue && row.Length.Value < 0)
                    {
                        rowErrors.Add("Length cannot be negative");
                    }

                    if (row.Width.HasValue && row.Width.Value < 0)
                    {
                        rowErrors.Add("Width cannot be negative");
                    }

                    if (row.Height.HasValue && row.Height.Value < 0)
                    {
                        rowErrors.Add("Height cannot be negative");
                    }

                    // Validate status
                    var validStatuses = new[] { ProductStatus.Draft, ProductStatus.Published, ProductStatus.Archived };
                    if (!validStatuses.Contains(row.Status))
                    {
                        rowErrors.Add($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");
                    }

                    // Look up category
                    Category? category = null;
                    if (!string.IsNullOrWhiteSpace(row.Category))
                    {
                        var categoryKey = row.Category.ToLower();
                        if (!categories.TryGetValue(categoryKey, out category))
                        {
                            rowErrors.Add($"Category '{row.Category}' not found or inactive");
                        }
                    }

                    // Parse image URLs
                    List<string>? imageUrls = null;
                    string imageUrlsJson = "[]";
                    if (!string.IsNullOrWhiteSpace(row.ImageUrls))
                    {
                        imageUrls = row.ImageUrls.Split('|', StringSplitOptions.RemoveEmptyEntries)
                            .Select(url => url.Trim())
                            .ToList();
                        imageUrlsJson = JsonSerializer.Serialize(imageUrls);
                        
                        // Validate ImageUrls field length constraint
                        if (imageUrlsJson.Length > 2000)
                        {
                            rowErrors.Add("Image URLs total length exceeds maximum allowed (2000 characters)");
                        }
                    }

                    // Validate published products have at least one image
                    if (row.Status == ProductStatus.Published && (imageUrls == null || imageUrls.Count == 0))
                    {
                        rowErrors.Add("Published products must have at least one image");
                    }

                    // If there are validation errors, record them and continue
                    if (rowErrors.Count > 0)
                    {
                        result.Errors.Add(new ProductCsvRowError
                        {
                            RowNumber = rowNumber,
                            SKU = row.SKU,
                            ErrorMessages = rowErrors
                        });
                        result.FailureCount++;
                        continue;
                    }

                    // Check if product exists (update) or create new
                    Product product;
                    if (existingProducts.TryGetValue(row.SKU, out var existingProduct))
                    {
                        // Update existing product
                        product = existingProduct;
                        product.Title = row.Title;
                        product.Description = row.Description;
                        product.CategoryId = category!.Id;
                        product.Price = row.Price;
                        product.StockQuantity = row.StockQuantity;
                        product.Weight = row.Weight;
                        product.Length = row.Length;
                        product.Width = row.Width;
                        product.Height = row.Height;
                        product.ImageUrls = imageUrlsJson;
                        product.Status = row.Status;
                        product.UpdatedAt = DateTime.UtcNow;
                        _context.Entry(product).State = EntityState.Modified;
                    }
                    else
                    {
                        // Create new product
                        product = new Product
                        {
                            Id = Guid.NewGuid(),
                            StoreId = storeId,
                            SKU = row.SKU,
                            Title = row.Title,
                            Description = row.Description,
                            CategoryId = category!.Id,
                            Price = row.Price,
                            Currency = "USD", // MVP: USD only
                            StockQuantity = row.StockQuantity,
                            Weight = row.Weight,
                            Length = row.Length,
                            Width = row.Width,
                            Height = row.Height,
                            ImageUrls = imageUrlsJson,
                            Status = row.Status,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Products.Add(product);
                        existingProducts[row.SKU] = product;
                    }

                    recordsInBatch++;

                    // Save in batches to reduce transaction size and improve performance
                    if (recordsInBatch >= batchSize)
                    {
                        try
                        {
                            await _context.SaveChangesAsync();
                            result.SuccessCount += recordsInBatch;
                            recordsInBatch = 0;
                        }
                        catch (DbUpdateException dbEx)
                        {
                            _logger.LogError(dbEx, "Database error saving batch ending at row {RowNumber}", rowNumber);
                            // When batch save fails, we can't determine which specific row caused it
                            // Report it as a batch error
                            result.Errors.Add(new ProductCsvRowError
                            {
                                RowNumber = rowNumber,
                                SKU = "BATCH ERROR",
                                ErrorMessages = new List<string> { $"Database error saving batch of {recordsInBatch} records. Batch not saved." }
                            });
                            result.FailureCount += recordsInBatch;
                            // Clear the context to recover from the error
                            _context.ChangeTracker.Clear();
                            recordsInBatch = 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing row {RowNumber} with SKU {SKU}", rowNumber, row.SKU);
                    result.Errors.Add(new ProductCsvRowError
                    {
                        RowNumber = rowNumber,
                        SKU = row.SKU,
                        ErrorMessages = new List<string> { $"Unexpected error: {ex.Message}" }
                    });
                    result.FailureCount++;
                }

                rowNumber++;
            }

            // Save any remaining changes from the last batch
            if (recordsInBatch > 0)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    result.SuccessCount += recordsInBatch;
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Database error saving final batch for store {StoreId}", storeId);
                    result.Errors.Add(new ProductCsvRowError
                    {
                        RowNumber = rowNumber - 1, // Last processed row
                        SKU = "BATCH ERROR",
                        ErrorMessages = new List<string> { $"Database error saving final batch of {recordsInBatch} records. Batch not saved." }
                    });
                    result.FailureCount += recordsInBatch;
                }
            }

            result.Success = result.FailureCount == 0;
            result.Message = result.Success
                ? $"Successfully imported {result.SuccessCount} product(s)"
                : $"Imported {result.SuccessCount} product(s) with {result.FailureCount} error(s)";

            _logger.LogInformation(
                "CSV import completed for store {StoreId}: {SuccessCount} succeeded, {FailureCount} failed",
                storeId, result.SuccessCount, result.FailureCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing CSV for store {StoreId}", storeId);
            result.Success = false;
            result.Message = $"CSV import failed: {ex.Message}";
            return result;
        }
    }

    public async Task<byte[]> ExportProductsAsync(Guid storeId, ExportProductsCsvRequest? request = null)
    {
        try
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.StoreId == storeId);

            // Apply filters if provided
            if (request != null)
            {
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    query = query.Where(p => p.Status == request.Status);
                }

                if (request.CategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == request.CategoryId.Value);
                }
            }

            var products = await query.OrderBy(p => p.SKU).ToListAsync();

            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };
            using var csv = new CsvWriter(writer, config);

            // Write CSV records
            var csvRows = products.Select(p => new ProductCsvRow
            {
                SKU = p.SKU,
                Title = p.Title,
                Description = p.Description,
                Category = p.Category?.Name ?? string.Empty,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Weight = p.Weight,
                Length = p.Length,
                Width = p.Width,
                Height = p.Height,
                ImageUrls = ConvertImageUrlsToString(p.ImageUrls),
                Status = p.Status
            });

            csv.WriteRecords(csvRows);
            await writer.FlushAsync();

            _logger.LogInformation("Exported {Count} products for store {StoreId}", products.Count, storeId);

            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting products for store {StoreId}", storeId);
            throw;
        }
    }

    public byte[] GenerateTemplate(bool includeSampleRow = true)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };
        using var csv = new CsvWriter(writer, config);

        var rows = new List<ProductCsvRow>();

        if (includeSampleRow)
        {
            rows.Add(new ProductCsvRow
            {
                SKU = "SAMPLE-001",
                Title = "Sample Product",
                Description = "This is a sample product description. Replace with your product details.",
                Category = "Electronics",
                Price = 99.99m,
                StockQuantity = 100,
                Weight = 500,
                Length = 20,
                Width = 15,
                Height = 10,
                ImageUrls = "https://example.com/image1.jpg|https://example.com/image2.jpg",
                Status = ProductStatus.Draft
            });
        }

        csv.WriteRecords(rows);
        writer.Flush();

        return memoryStream.ToArray();
    }

    private string? ConvertImageUrlsToString(string imageUrlsJson)
    {
        if (string.IsNullOrWhiteSpace(imageUrlsJson) || imageUrlsJson == "[]")
        {
            return null;
        }

        try
        {
            var urls = JsonSerializer.Deserialize<List<string>>(imageUrlsJson);
            return urls != null && urls.Count > 0 ? string.Join("|", urls) : null;
        }
        catch
        {
            return null;
        }
    }
}
