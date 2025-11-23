using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.Reports.DTOs;
using SD.Mercato.Reports.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for managing global commission configuration.
/// Admin-only endpoints.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class CommissionConfigController : ControllerBase
{
    private readonly ICommissionConfigService _configService;
    private readonly ILogger<CommissionConfigController> _logger;

    public CommissionConfigController(
        ICommissionConfigService configService,
        ILogger<CommissionConfigController> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the active global commission configuration.
    /// </summary>
    /// <returns>Global commission configuration</returns>
    [HttpGet]
    public async Task<IActionResult> GetActiveConfig()
    {
        _logger.LogInformation("Admin {UserId} requesting active commission configuration",
            User.FindFirstValue(ClaimTypes.NameIdentifier));

        var config = await _configService.GetActiveConfigAsync();

        if (config == null)
        {
            return NotFound(new { message = "No active commission configuration found" });
        }

        return Ok(config);
    }

    /// <summary>
    /// Updates the global commission configuration.
    /// </summary>
    /// <param name="request">Update request with new commission rate</param>
    /// <returns>Updated configuration</returns>
    [HttpPut]
    public async Task<IActionResult> UpdateConfig([FromBody] UpdateCommissionConfigRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        _logger.LogInformation("Admin {UserId} updating commission configuration to {Rate}",
            userId, request.DefaultCommissionRate);

        try
        {
            var requestWithUser = request with { ModifiedBy = userId };
            var updatedConfig = await _configService.UpdateConfigAsync(requestWithUser);

            return Ok(updatedConfig);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating commission configuration");
            return StatusCode(500, new { message = "An error occurred while updating configuration" });
        }
    }
}
