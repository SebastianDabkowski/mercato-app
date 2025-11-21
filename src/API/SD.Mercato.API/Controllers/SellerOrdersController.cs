using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.History.DTOs;
using SD.Mercato.History.Services;
using SD.Mercato.SellerPanel.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for seller order management operations.
/// Sellers can view and manage their SubOrders.
/// </summary>
[ApiController]
[Route("api/seller/orders")]
[Authorize(Roles = "Seller")]
public class SellerOrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IStoreService _storeService;
    private readonly ILogger<SellerOrdersController> _logger;

    public SellerOrdersController(
        IOrderService orderService,
        IStoreService storeService,
        ILogger<SellerOrdersController> logger)
    {
        _orderService = orderService;
        _storeService = storeService;
        _logger = logger;
    }

    /// <summary>
    /// Get all SubOrders for the authenticated seller's store with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SubOrderListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubOrderListResponse>> GetSubOrders(
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

        // Get seller's store
        var store = await _storeService.GetStoreByOwnerIdAsync(userId);
        if (store == null)
        {
            return NotFound(new { message = "Store not found for this seller" });
        }

        var filter = new SubOrderFilterRequest
        {
            Status = status,
            FromDate = fromDate,
            ToDate = toDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await _orderService.GetStoreSubOrdersAsync(store.Id, filter);
        return Ok(result);
    }

    /// <summary>
    /// Get SubOrder details by ID.
    /// </summary>
    [HttpGet("{subOrderId:guid}")]
    [ProducesResponseType(typeof(SubOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SubOrderDto>> GetSubOrderById(Guid subOrderId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        // Get seller's store
        var store = await _storeService.GetStoreByOwnerIdAsync(userId);
        if (store == null)
        {
            return NotFound(new { message = "Store not found for this seller" });
        }

        var subOrder = await _orderService.GetSubOrderByIdAsync(subOrderId, store.Id);
        if (subOrder == null)
        {
            return NotFound(new { message = "SubOrder not found" });
        }

        return Ok(subOrder);
    }

    /// <summary>
    /// Update SubOrder status.
    /// </summary>
    [HttpPut("{subOrderId:guid}/status")]
    [ProducesResponseType(typeof(SubOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SubOrderDto>> UpdateSubOrderStatus(
        Guid subOrderId,
        [FromBody] UpdateSubOrderStatusRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        // Get seller's store
        var store = await _storeService.GetStoreByOwnerIdAsync(userId);
        if (store == null)
        {
            return NotFound(new { message = "Store not found for this seller" });
        }

        // Update status
        var (success, errorMessage) = await _orderService.UpdateSubOrderStatusAsync(
            subOrderId, 
            store.Id, 
            request);

        if (!success)
        {
            return BadRequest(new { message = errorMessage });
        }

        // Return updated SubOrder
        var updatedSubOrder = await _orderService.GetSubOrderByIdAsync(subOrderId, store.Id);
        if (updatedSubOrder == null)
        {
            return NotFound(new { message = "SubOrder not found after update" });
        }

        return Ok(updatedSubOrder);
    }
}
