using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.History.DTOs;
using SD.Mercato.History.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for order management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders for the authenticated user with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(OrderListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderListResponse>> GetUserOrders(
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
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
        return Ok(result);
    }

    /// <summary>
    /// Get order details by ID.
    /// </summary>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid orderId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var order = await _orderService.GetOrderByIdAsync(orderId, userId);
        if (order == null)
        {
            return NotFound(new { message = "Order not found" });
        }

        return Ok(order);
    }
}
