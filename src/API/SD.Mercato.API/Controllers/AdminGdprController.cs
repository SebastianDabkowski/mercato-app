using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.Administration.DTOs;
using SD.Mercato.Administration.Services;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Admin controller for GDPR compliance operations.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Administrator")]
public class AdminGdprController : ControllerBase
{
    private readonly IAdminGdprService _gdprService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AdminGdprController> _logger;

    public AdminGdprController(
        IAdminGdprService gdprService,
        IAuditLogService auditLogService,
        ILogger<AdminGdprController> logger)
    {
        _gdprService = gdprService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Get all user data for GDPR compliance audit.
    /// Used by privacy officers to respond to data subject access requests.
    /// </summary>
    [HttpPost("user-data")]
    [ProducesResponseType(typeof(AdminGdprUserDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminGdprUserDataResponse>> GetUserData([FromBody] AdminGdprSearchRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrEmpty(request.UserId) && string.IsNullOrEmpty(request.Email))
        {
            return BadRequest(new { message = "Either UserId or Email must be provided" });
        }

        var adminUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        var adminEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "Unknown";
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var userData = await _gdprService.GetUserDataAsync(request);
        if (userData == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // Audit log the access
        await _auditLogService.LogActionAsync(
            adminUserId,
            adminEmail,
            "GDPR_DATA_ACCESS",
            "User",
            userData.User.Id,
            $"Admin accessed GDPR data for user {userData.User.Email}",
            null,
            ipAddress);

        _logger.LogInformation(
            "Admin {AdminEmail} accessed GDPR data for user {UserId} ({UserEmail})",
            adminEmail,
            userData.User.Id,
            userData.User.Email);

        return Ok(userData);
    }
}
