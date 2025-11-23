using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.Reports.DTOs;
using SD.Mercato.Reports.Services;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for admin dashboard metrics.
/// Provides overview metrics for marketplace administrators.
/// </summary>
[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Administrator")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(
        IAdminDashboardService dashboardService,
        ILogger<AdminDashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Gets admin dashboard metrics for the marketplace.
    /// </summary>
    /// <returns>Admin dashboard metrics including GMV, orders, sellers, and registrations.</returns>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(AdminDashboardMetrics), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminDashboardMetrics>> GetDashboardMetrics()
    {
        _logger.LogInformation("Admin requesting dashboard metrics");

        try
        {
            var metrics = await _dashboardService.GetDashboardMetricsAsync();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin dashboard metrics");
            return StatusCode(500, new { message = "An error occurred while retrieving dashboard metrics" });
        }
    }
}
