using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.Reports.DTOs;
using SD.Mercato.Reports.Services;
using SD.Mercato.SellerPanel.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for seller dashboard metrics.
/// Provides sales metrics and best-selling products for sellers.
/// </summary>
[ApiController]
[Route("api/seller/dashboard")]
[Authorize(Roles = "Seller")]
public class SellerDashboardController : ControllerBase
{
    private readonly ISellerDashboardService _dashboardService;
    private readonly IStoreService _storeService;
    private readonly ILogger<SellerDashboardController> _logger;

    public SellerDashboardController(
        ISellerDashboardService dashboardService,
        IStoreService storeService,
        ILogger<SellerDashboardController> logger)
    {
        _dashboardService = dashboardService;
        _storeService = storeService;
        _logger = logger;
    }

    /// <summary>
    /// Gets seller dashboard metrics for a specific period.
    /// </summary>
    /// <param name="storeId">Store ID</param>
    /// <param name="startDate">Period start date (inclusive)</param>
    /// <param name="endDate">Period end date (inclusive)</param>
    /// <returns>Seller dashboard metrics including sales, orders, and best-selling products.</returns>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(SellerDashboardMetrics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SellerDashboardMetrics>> GetDashboardMetrics(
        [FromQuery] Guid storeId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        _logger.LogInformation(
            "Seller {UserId} requesting dashboard metrics for Store {StoreId}",
            User.FindFirstValue(ClaimTypes.NameIdentifier), storeId);

        // Validate date range
        if (startDate > endDate)
        {
            return BadRequest(new { message = "Start date must be before or equal to end date" });
        }

        // Verify store ownership
        if (!await VerifyStoreOwnershipAsync(storeId))
        {
            _logger.LogWarning(
                "Unauthorized access attempt to Store {StoreId} by User {UserId}",
                storeId, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Forbid();
        }

        try
        {
            var request = new SellerDashboardRequest
            {
                StoreId = storeId,
                // Convert to UTC and create exclusive upper bound (end of day)
                // StartDate is inclusive (beginning of day), EndDate+1 is exclusive (beginning of next day)
                StartDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(endDate.Date.AddDays(1), DateTimeKind.Utc)
            };

            var metrics = await _dashboardService.GetDashboardMetricsAsync(request);

            if (metrics == null)
            {
                return NotFound(new { message = "Store not found or no data available" });
            }

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving seller dashboard metrics for Store {StoreId}", storeId);
            return StatusCode(500, new { message = "An error occurred while retrieving dashboard metrics" });
        }
    }

    /// <summary>
    /// Verifies that the authenticated user owns the specified store.
    /// </summary>
    private async Task<bool> VerifyStoreOwnershipAsync(Guid storeId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        var store = await _storeService.GetStoreByIdAsync(storeId);
        return store != null && store.OwnerUserId == userId;
    }
}
