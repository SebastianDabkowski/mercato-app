using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.History.DTOs;
using SD.Mercato.History.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for checkout and order creation operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<CheckoutController> _logger;

    public CheckoutController(IOrderService orderService, ILogger<CheckoutController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Create an order from the user's cart and redirect to payment.
    /// </summary>
    [HttpPost("create-order")]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreateOrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
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

        var result = await _orderService.CreateOrderFromCartAsync(userId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Order created via checkout: OrderId={OrderId}, UserId={UserId}",
            result.OrderId, userId);

        return Ok(result);
    }

    /// <summary>
    /// Update payment status (called by payment gateway webhook or payment confirmation page).
    /// </summary>
    [HttpPost("payment-callback")]
    [AllowAnonymous] // Payment gateway may call this endpoint
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PaymentCallback([FromBody] PaymentCallbackRequest request)
    {
        // TODO: Validate payment gateway signature to prevent fraud
        // TODO: Use proper payment gateway verification instead of accepting any callback

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _orderService.UpdatePaymentStatusAsync(
            request.OrderId,
            request.PaymentStatus,
            request.TransactionId);

        if (!updated)
        {
            return BadRequest(new { message = "Failed to update payment status" });
        }

        _logger.LogInformation("Payment callback processed: OrderId={OrderId}, Status={Status}",
            request.OrderId, request.PaymentStatus);

        return Ok(new { message = "Payment status updated" });
    }
}

/// <summary>
/// Payment callback request model (MVP stub for payment gateway integration).
/// </summary>
public class PaymentCallbackRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    public Guid OrderId { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    public string PaymentStatus { get; set; } = string.Empty;

    public string? TransactionId { get; set; }
}
