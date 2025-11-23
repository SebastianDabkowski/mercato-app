using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.ProductCatalog.DTOs;
using SD.Mercato.ProductCatalog.Services;
using SD.Mercato.SellerPanel.Services;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Public API controller for read-only access to product catalog.
/// No authentication required.
/// </summary>
[ApiController]
[Route("api/public")]
[AllowAnonymous]
public class PublicApiController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IStoreService _storeService;
    private readonly ILogger<PublicApiController> _logger;

    public PublicApiController(
        IProductService productService,
        IStoreService storeService,
        ILogger<PublicApiController> logger)
    {
        _productService = productService;
        _storeService = storeService;
        _logger = logger;
    }

    /// <summary>
    /// Get all published products from all stores (global catalog).
    /// </summary>
    /// <remarks>
    /// Public endpoint - no authentication required.
    /// Returns only published products.
    /// </remarks>
    [HttpGet("products")]
    [ProducesResponseType(typeof(List<PublicProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PublicProductDto>>> GetCatalog()
    {
        var products = await _productService.GetAllPublishedProductsAsync();
        await PopulateStoreNamesAsync(products);
        return Ok(products);
    }

    /// <summary>
    /// Get a specific product by ID.
    /// </summary>
    /// <remarks>
    /// Public endpoint - no authentication required.
    /// Returns product details if published.
    /// </remarks>
    [HttpGet("products/{productId:guid}")]
    [ProducesResponseType(typeof(PublicProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicProductDto>> GetProduct(Guid productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);

        if (product == null)
        {
            return NotFound(new { message = "Product not found" });
        }

        // Only return if published
        if (product.Status != "Published")
        {
            return NotFound(new { message = "Product not found" });
        }

        // Convert to PublicProductDto
        var publicProduct = new PublicProductDto
        {
            Id = product.Id,
            StoreId = product.StoreId,
            Title = product.Title,
            Description = product.Description,
            Price = product.Price,
            CategoryName = product.CategoryName,
            ImageUrls = product.ImageUrls,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt
        };

        // Populate store name
        var store = await _storeService.GetStoreByIdAsync(product.StoreId);
        if (store != null)
        {
            publicProduct.StoreName = store.DisplayName ?? store.StoreName;
        }

        return Ok(publicProduct);
    }

    /// <summary>
    /// Get published products for a specific store.
    /// </summary>
    /// <remarks>
    /// Public endpoint - no authentication required.
    /// Returns only published products from the specified store.
    /// </remarks>
    [HttpGet("stores/{storeId:guid}/products")]
    [ProducesResponseType(typeof(List<PublicProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PublicProductDto>>> GetStoreProducts(Guid storeId)
    {
        var products = await _productService.GetPublishedProductsByStoreIdAsync(storeId);
        await PopulateStoreNamesAsync(products);
        return Ok(products);
    }

    /// <summary>
    /// Search and filter products with pagination.
    /// </summary>
    /// <remarks>
    /// Public endpoint - no authentication required.
    /// Supports filtering by category, price range, search term, and pagination.
    /// </remarks>
    [HttpPost("products/search")]
    [ProducesResponseType(typeof(PaginatedProductsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedProductsResponse>> SearchProducts([FromBody] ProductSearchRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validate MinPrice vs MaxPrice relationship
        if (request.MinPrice.HasValue && request.MaxPrice.HasValue && request.MinPrice > request.MaxPrice)
        {
            ModelState.AddModelError("MinPrice", "Minimum price cannot be greater than maximum price");
            return BadRequest(ModelState);
        }

        var result = await _productService.SearchProductsAsync(request);
        await PopulateStoreNamesAsync(result.Products);
        return Ok(result);
    }

    /// <summary>
    /// Helper method to populate store names for product DTOs.
    /// </summary>
    private async Task PopulateStoreNamesAsync(List<PublicProductDto> products)
    {
        if (products.Count == 0) return;

        // Get unique store IDs
        var storeIds = products.Select(p => p.StoreId).Distinct().ToList();

        // TODO: Optimize N+1 query pattern by implementing batch method
        // Consider adding IStoreService.GetStoresByIdsAsync(List<Guid> storeIds)
        // to fetch all stores in a single database query

        // Fetch store information for all unique stores
        var storeDict = new Dictionary<Guid, string>();
        foreach (var storeId in storeIds)
        {
            var store = await _storeService.GetStoreByIdAsync(storeId);
            if (store != null)
            {
                storeDict[storeId] = store.DisplayName ?? store.StoreName;
            }
        }

        // Populate store names
        foreach (var product in products)
        {
            product.StoreName = storeDict.TryGetValue(product.StoreId, out var storeName)
                ? storeName
                : "Unknown Store";
        }
    }
}
