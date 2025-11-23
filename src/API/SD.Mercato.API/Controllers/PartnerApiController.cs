using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SD.Mercato.History.DTOs;
using SD.Mercato.History.Services;
using SD.Mercato.ProductCatalog.DTOs;
using SD.Mercato.ProductCatalog.Services;
using SD.Mercato.Users.Authentication;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Partner API controller for external integrations.
/// Requires API key authentication via X-API-Key header.
/// </summary>
[ApiController]
[Route("api/partner")]
[Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
public class PartnerApiController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PartnerApiController> _logger;

    public PartnerApiController(
        IProductService productService,
        IOrderService orderService,
        IConfiguration configuration,
        ILogger<PartnerApiController> logger)
    {
        _productService = productService;
        _orderService = orderService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Get all products for the authenticated partner's store.
    /// </summary>
    /// <remarks>
    /// Authentication: API key via X-API-Key header
    /// Permissions required: products:read
    /// </remarks>
    [HttpGet("products")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ProductDto>>> GetProducts()
    {
        if (!HasPermission("products:read"))
        {
            return Forbid();
        }

        var storeId = GetStoreIdFromClaims();
        if (!storeId.HasValue)
        {
            return StatusCode(403, new { message = "API token is not scoped to a store" });
        }

        var products = await _productService.GetProductsByStoreIdAsync(storeId.Value);
        return Ok(products);
    }

    /// <summary>
    /// Create a new product via partner API.
    /// </summary>
    /// <remarks>
    /// Authentication: API key via X-API-Key header
    /// Permissions required: products:write
    /// Feature flag: PartnerApi:EnableWrite must be true
    /// </remarks>
    [HttpPost("products")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check feature flag
        if (!IsWriteEnabled())
        {
            return StatusCode(403, new { message = "Partner API write operations are currently disabled" });
        }

        if (!HasPermission("products:write"))
        {
            return Forbid();
        }

        var storeId = GetStoreIdFromClaims();
        if (!storeId.HasValue)
        {
            return StatusCode(403, new { message = "API token is not scoped to a store" });
        }

        var result = await _productService.CreateProductAsync(storeId.Value, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Product created via partner API: {ProductId} for store {StoreId}", result.Product?.Id, storeId);

        return CreatedAtAction(nameof(GetProduct), new { productId = result.Product!.Id }, result);
    }

    /// <summary>
    /// Get a specific product by ID.
    /// </summary>
    /// <remarks>
    /// Authentication: API key via X-API-Key header
    /// Permissions required: products:read
    /// </remarks>
    [HttpGet("products/{productId:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid productId)
    {
        if (!HasPermission("products:read"))
        {
            return Forbid();
        }

        var storeId = GetStoreIdFromClaims();
        if (!storeId.HasValue)
        {
            return StatusCode(403, new { message = "API token is not scoped to a store" });
        }

        var product = await _productService.GetProductByIdAsync(productId);

        if (product == null)
        {
            return NotFound(new { message = "Product not found" });
        }

        // Verify product belongs to the store
        if (product.StoreId != storeId.Value)
        {
            return Forbid();
        }

        return Ok(product);
    }

    /// <summary>
    /// Update a product via partner API.
    /// </summary>
    /// <remarks>
    /// Authentication: API key via X-API-Key header
    /// Permissions required: products:write
    /// Feature flag: PartnerApi:EnableWrite must be true
    /// </remarks>
    [HttpPut("products/{productId:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(Guid productId, [FromBody] UpdateProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check feature flag
        if (!IsWriteEnabled())
        {
            return StatusCode(403, new { message = "Partner API write operations are currently disabled" });
        }

        if (!HasPermission("products:write"))
        {
            return Forbid();
        }

        var storeId = GetStoreIdFromClaims();
        if (!storeId.HasValue)
        {
            return StatusCode(403, new { message = "API token is not scoped to a store" });
        }

        var result = await _productService.UpdateProductAsync(productId, storeId.Value, request);

        if (!result.Success)
        {
            if (result.Message?.Contains("not found") == true)
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        _logger.LogInformation("Product updated via partner API: {ProductId} for store {StoreId}", productId, storeId);

        return Ok(result);
    }

    /// <summary>
    /// Update product stock level via partner API.
    /// </summary>
    /// <remarks>
    /// Authentication: API key via X-API-Key header
    /// Permissions required: products:write
    /// Feature flag: PartnerApi:EnableWrite must be true
    /// </remarks>
    [HttpPatch("products/{productId:guid}/stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateProductStock(Guid productId, [FromBody] UpdateStockRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check feature flag
        if (!IsWriteEnabled())
        {
            return StatusCode(403, new { message = "Partner API write operations are currently disabled" });
        }

        if (!HasPermission("products:write"))
        {
            return Forbid();
        }

        var storeId = GetStoreIdFromClaims();
        if (!storeId.HasValue)
        {
            return StatusCode(403, new { message = "API token is not scoped to a store" });
        }

        // Get existing product to verify ownership
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
        {
            return NotFound(new { message = "Product not found" });
        }

        if (product.StoreId != storeId.Value)
        {
            return Forbid();
        }

        // Update stock using the existing service
        var updateRequest = new UpdateProductRequest
        {
            StockQuantity = request.StockQuantity
        };

        var result = await _productService.UpdateProductAsync(productId, storeId.Value, updateRequest);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Product stock updated via partner API: {ProductId} to {Quantity}", productId, request.StockQuantity);

        return Ok(new { message = "Stock updated successfully", stockQuantity = request.StockQuantity });
    }

    /// <summary>
    /// Get orders for the authenticated partner's store.
    /// </summary>
    /// <remarks>
    /// Authentication: API key via X-API-Key header
    /// Permissions required: orders:read
    /// </remarks>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetOrders(
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (!HasPermission("orders:read"))
        {
            return Forbid();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var filter = new OrderFilterRequest
        {
            Status = status,
            FromDate = fromDate,
            ToDate = toDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await _orderService.GetUserOrdersAsync(userId, filter);

        _logger.LogInformation("Orders retrieved via partner API for user {UserId}", userId);

        return Ok(result);
    }

    /// <summary>
    /// Request DTO for updating stock.
    /// </summary>
    public class UpdateStockRequest
    {
        /// <summary>
        /// New stock quantity.
        /// </summary>
        public int StockQuantity { get; set; }
    }

    /// <summary>
    /// Check if the user has a specific permission.
    /// </summary>
    private bool HasPermission(string permission)
    {
        var permissions = User.FindAll("Permission").Select(c => c.Value);
        return permissions.Contains(permission);
    }

    /// <summary>
    /// Get the store ID from claims.
    /// </summary>
    private Guid? GetStoreIdFromClaims()
    {
        var storeIdClaim = User.FindFirst("StoreId")?.Value;
        if (string.IsNullOrEmpty(storeIdClaim))
        {
            return null;
        }

        if (Guid.TryParse(storeIdClaim, out var storeId))
        {
            return storeId;
        }

        return null;
    }

    /// <summary>
    /// Check if write operations are enabled via feature flag.
    /// </summary>
    private bool IsWriteEnabled()
    {
        var enableWrite = _configuration.GetValue<bool>("PartnerApi:EnableWrite", false);
        return enableWrite;
    }
}
