using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.Cart.DTOs;
using SD.Mercato.Cart.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for shopping cart operations.
/// Supports both authenticated users and guest sessions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(ICartService cartService, ILogger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    /// <summary>
    /// Get the current user's cart or guest cart.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var (userId, sessionId) = GetUserOrSessionId();

        var cart = await _cartService.GetCartAsync(userId, sessionId);

        if (cart == null)
        {
            return NotFound(new { message = "Cart not found" });
        }

        return Ok(cart);
    }

    /// <summary>
    /// Add an item to the cart.
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(typeof(AddToCartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AddToCartResponse>> AddItem([FromBody] AddToCartRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (userId, sessionId) = GetUserOrSessionId();

        var result = await _cartService.AddItemAsync(userId, sessionId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Item added to cart: ProductId={ProductId}, UserId={UserId}, SessionId={SessionId}",
            request.ProductId, userId ?? "null", sessionId ?? "null");

        return Ok(result);
    }

    /// <summary>
    /// Update the quantity of a cart item.
    /// </summary>
    [HttpPut("items/{cartItemId:guid}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> UpdateItemQuantity(Guid cartItemId, [FromBody] UpdateCartItemRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (userId, sessionId) = GetUserOrSessionId();

        var result = await _cartService.UpdateItemQuantityAsync(userId, sessionId, cartItemId, request);

        if (!result.Success)
        {
            // TODO: Use strongly-typed error codes instead of string matching for better error handling
            if (result.Message?.Contains("not found") == true)
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        _logger.LogInformation("Cart item quantity updated: CartItemId={CartItemId}, Quantity={Quantity}",
            cartItemId, request.Quantity);

        return Ok(result);
    }

    /// <summary>
    /// Remove an item from the cart.
    /// </summary>
    [HttpDelete("items/{cartItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(Guid cartItemId)
    {
        var (userId, sessionId) = GetUserOrSessionId();

        var removed = await _cartService.RemoveItemAsync(userId, sessionId, cartItemId);

        if (!removed)
        {
            return NotFound(new { message = "Cart item not found" });
        }

        _logger.LogInformation("Cart item removed: CartItemId={CartItemId}", cartItemId);

        return NoContent();
    }

    /// <summary>
    /// Clear all items from the cart.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClearCart()
    {
        var (userId, sessionId) = GetUserOrSessionId();

        var cleared = await _cartService.ClearCartAsync(userId, sessionId);

        if (!cleared)
        {
            return NotFound(new { message = "Cart not found" });
        }

        _logger.LogInformation("Cart cleared: UserId={UserId}, SessionId={SessionId}",
            userId ?? "null", sessionId ?? "null");

        return NoContent();
    }

    /// <summary>
    /// Migrate guest cart to user account upon login.
    /// Called by the authentication system after successful login.
    /// </summary>
    [HttpPost("migrate")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MigrateGuestCart([FromBody] MigrateCartRequest request)
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

        var migrated = await _cartService.MigrateGuestCartAsync(request.SessionId, userId);

        if (!migrated)
        {
            return BadRequest(new { message = "Failed to migrate cart" });
        }

        _logger.LogInformation("Guest cart migrated: SessionId={SessionId}, UserId={UserId}",
            request.SessionId, userId);

        return Ok(new { message = "Cart migrated successfully" });
    }

    /// <summary>
    /// Helper method to extract user ID or session ID from request.
    /// </summary>
    private (string? userId, string? sessionId) GetUserOrSessionId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // If authenticated, use user ID
        if (!string.IsNullOrEmpty(userId))
        {
            return (userId, null);
        }

        // Otherwise, try to get or create session ID from headers or cookies
        // TODO: Implement proper session management for guest carts
        // For now, use a header-based approach
        var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(sessionId))
        {
            // Generate a new session ID and include it in response headers
            sessionId = Guid.NewGuid().ToString();
            Response.Headers.Append("X-Session-Id", sessionId);
        }

        return (null, sessionId);
    }
}

/// <summary>
/// Request model for migrating a guest cart to a user account.
/// </summary>
public class MigrateCartRequest
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Session ID is required")]
    public string SessionId { get; set; } = string.Empty;
}
