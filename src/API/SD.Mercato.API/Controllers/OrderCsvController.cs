using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.History.DTOs;
using SD.Mercato.History.Services;
using SD.Mercato.SellerPanel.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for seller order CSV export operations.
/// </summary>
[ApiController]
[Route("api/seller/orders/csv")]
[Authorize(Roles = "Seller")]
public class OrderCsvController : ControllerBase
{
    private readonly IOrderCsvService _csvService;
    private readonly IStoreService _storeService;
    private readonly ILogger<OrderCsvController> _logger;

    public OrderCsvController(
        IOrderCsvService csvService,
        IStoreService storeService,
        ILogger<OrderCsvController> logger)
    {
        _csvService = csvService;
        _storeService = storeService;
        _logger = logger;
    }

    /// <summary>
    /// Export orders to CSV file for accounting and integration purposes.
    /// Includes commission calculations and net amounts.
    /// </summary>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportOrders(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? status = null)
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

        // Validate status if provided
        if (!string.IsNullOrEmpty(status))
        {
            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Completed", "Cancelled" };
            if (!validStatuses.Contains(status))
            {
                return BadRequest(new 
                { 
                    message = $"Invalid status. Valid values are: {string.Join(", ", validStatuses)}" 
                });
            }
        }

        // Validate date range
        if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
        {
            return BadRequest(new { message = "From date cannot be after to date" });
        }

        try
        {
            var request = new ExportOrdersCsvRequest
            {
                FromDate = fromDate,
                ToDate = toDate,
                Status = status
            };

            var result = await _csvService.ExportOrdersAsync(store.Id, request);

            if (!result.Success)
            {
                var correlationId = Guid.NewGuid();
                _logger.LogError("Order CSV export failed for store {StoreId}. CorrelationId: {CorrelationId}. Error: {ErrorMessage}", 
                    store.Id, correlationId, result.ErrorMessage);
                return StatusCode(500, new { message = "An error occurred while exporting orders", correlationId });
            }

            if (result.RecordCount == 0)
            {
                return Ok(new 
                { 
                    message = "No orders found matching the specified criteria",
                    recordCount = 0
                });
            }

            _logger.LogInformation(
                "Order CSV export for store {StoreId}: {RecordCount} records",
                store.Id, result.RecordCount);

            return File(result.FileContent!, "text/csv", result.FileName!);
        }
        catch (Exception ex)
        {
            var correlationId = Guid.NewGuid();
            _logger.LogError(ex, "Error exporting orders for store {StoreId}. CorrelationId: {CorrelationId}", store.Id, correlationId);
            return StatusCode(500, new { message = "An error occurred while exporting orders", correlationId });
        }
    }
}
