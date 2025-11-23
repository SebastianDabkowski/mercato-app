using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.ProductCatalog.DTOs;
using SD.Mercato.ProductCatalog.Services;
using SD.Mercato.SellerPanel.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for product CSV import/export operations.
/// </summary>
[ApiController]
[Route("api/products/csv")]
[Authorize(Roles = "Seller")]
public class ProductCsvController : ControllerBase
{
    private readonly IProductCsvService _csvService;
    private readonly IStoreService _storeService;
    private readonly ILogger<ProductCsvController> _logger;

    public ProductCsvController(
        IProductCsvService csvService,
        IStoreService storeService,
        ILogger<ProductCsvController> logger)
    {
        _csvService = csvService;
        _storeService = storeService;
        _logger = logger;
    }

    /// <summary>
    /// Download CSV template for product import.
    /// </summary>
    [HttpGet("template")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult DownloadTemplate([FromQuery] bool includeSample = true)
    {
        var templateBytes = _csvService.GenerateTemplate(includeSample);
        return File(templateBytes, "text/csv", "product_import_template.csv");
    }

    /// <summary>
    /// Import products from CSV file.
    /// Creates new products or updates existing ones based on SKU.
    /// </summary>
    [HttpPost("import")]
    [ProducesResponseType(typeof(ProductCsvImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductCsvImportResult>> ImportProducts(IFormFile file)
    {
        // Validate file
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided or file is empty" });
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "File must be a CSV file" });
        }

        // Get authenticated user
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        // Get user's store
        var store = await _storeService.GetStoreByOwnerIdAsync(userId);
        if (store == null)
        {
            return NotFound(new { message = "Store not found for this seller. Please create a store first." });
        }

        // Process import
        try
        {
            using var stream = file.OpenReadStream();
            var result = await _csvService.ImportProductsAsync(store.Id, stream);

            _logger.LogInformation(
                "CSV import for store {StoreId}: {SuccessCount} succeeded, {FailureCount} failed",
                store.Id, result.SuccessCount, result.FailureCount);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CSV import for store {StoreId}", store.Id);
            return StatusCode(500, new { message = "An error occurred while processing the CSV file", error = ex.Message });
        }
    }

    /// <summary>
    /// Export products to CSV file.
    /// </summary>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportProducts(
        [FromQuery] string? status = null,
        [FromQuery] Guid? categoryId = null)
    {
        // Get authenticated user
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        // Get user's store
        var store = await _storeService.GetStoreByOwnerIdAsync(userId);
        if (store == null)
        {
            return NotFound(new { message = "Store not found for this seller" });
        }

        try
        {
            var request = new ExportProductsCsvRequest
            {
                Status = status,
                CategoryId = categoryId
            };

            var csvBytes = await _csvService.ExportProductsAsync(store.Id, request);

            var fileName = $"products_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

            _logger.LogInformation("Product CSV export for store {StoreId}", store.Id);

            return File(csvBytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting products for store {StoreId}", store.Id);
            return StatusCode(500, new { message = "An error occurred while exporting products", error = ex.Message });
        }
    }
}
