using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.ProductCatalog.DTOs;
using SD.Mercato.ProductCatalog.Services;
using SD.Mercato.SellerPanel.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for product management endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IStoreService _storeService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        IStoreService storeService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _storeService = storeService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new product (seller only).
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        // Get the user's store
        var store = await _storeService.GetStoreByOwnerIdAsync(userId);
        if (store == null)
        {
            return Forbid();
        }

        var result = await _productService.CreateProductAsync(store.Id, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Product created: {ProductId} by user {UserId}", result.Product?.Id, userId);
        return Ok(result);
    }

    /// <summary>
    /// Update a product (seller only).
    /// </summary>
    [HttpPut("{productId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(Guid productId, [FromBody] UpdateProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        // Get the user's store
        var store = await _storeService.GetStoreByOwnerIdAsync(userId);
        if (store == null)
        {
            return Forbid();
        }

        var result = await _productService.UpdateProductAsync(productId, store.Id, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        _logger.LogInformation("Product updated: {ProductId} by user {UserId}", productId, userId);
        return Ok(result);
    }

    /// <summary>
    /// Get a product by ID.
    /// </summary>
    [HttpGet("{productId:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProductById(Guid productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);

        if (product == null)
        {
            return NotFound(new { message = "Product not found" });
        }

        return Ok(product);
    }

    /// <summary>
    /// Get all products for the authenticated seller's store.
    /// </summary>
    [HttpGet("my-products")]
    [Authorize]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ProductDto>>> GetMyProducts()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        // Get the user's store
        var store = await _storeService.GetStoreByOwnerIdAsync(userId);
        if (store == null)
        {
            return Forbid();
        }

        var products = await _productService.GetProductsByStoreIdAsync(store.Id);
        return Ok(products);
    }

    /// <summary>
    /// Get published products for a store (public catalog).
    /// </summary>
    [HttpGet("store/{storeId:guid}")]
    [ProducesResponseType(typeof(List<PublicProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PublicProductDto>>> GetStoreProducts(Guid storeId)
    {
        var products = await _productService.GetPublishedProductsByStoreIdAsync(storeId);
        return Ok(products);
    }

    /// <summary>
    /// Delete a product (seller only).
    /// </summary>
    [HttpDelete("{productId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid productId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        // Get the user's store
        var store = await _storeService.GetStoreByOwnerIdAsync(userId);
        if (store == null)
        {
            return Forbid();
        }

        var deleted = await _productService.DeleteProductAsync(productId, store.Id);

        if (!deleted)
        {
            return NotFound(new { message = "Product not found or you do not have permission to delete it" });
        }

        _logger.LogInformation("Product deleted: {ProductId} by user {UserId}", productId, userId);
        return NoContent();
    }
}
