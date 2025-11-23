using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SD.Mercato.Administration.DTOs;
using SD.Mercato.Administration.Services;
using System.Security.Claims;

namespace SD.Mercato.API.Controllers;

/// <summary>
/// Controller for admin user management endpoints.
/// </summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Administrator")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(
        IAdminUserService adminUserService,
        ILogger<AdminUsersController> logger)
    {
        _adminUserService = adminUserService;
        _logger = logger;
    }

    /// <summary>
    /// Search and list users with filtering and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedUsersResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedUsersResponse>> SearchUsers([FromQuery] AdminUserSearchRequest request)
    {
        var result = await _adminUserService.SearchUsersAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get detailed user information.
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(AdminUserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDetailDto>> GetUserDetail(string userId)
    {
        var user = await _adminUserService.GetUserDetailAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    /// <summary>
    /// Activate or deactivate a user account.
    /// </summary>
    [HttpPut("{userId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserStatus(string userId, [FromBody] UpdateUserStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(adminUserId) || string.IsNullOrEmpty(adminEmail))
        {
            return Unauthorized();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _adminUserService.UpdateUserStatusAsync(
            userId,
            request,
            adminUserId,
            adminEmail,
            ipAddress);

        if (!result)
        {
            return NotFound(new { message = "User not found" });
        }

        _logger.LogInformation("User {UserId} status updated to {Status} by admin {AdminId}",
            userId, request.IsActive ? "Active" : "Inactive", adminUserId);

        return Ok(new { message = "User status updated successfully" });
    }

    /// <summary>
    /// Send password reset email to a user.
    /// </summary>
    [HttpPost("password-reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendPasswordReset([FromBody] AdminPasswordResetRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(adminUserId) || string.IsNullOrEmpty(adminEmail))
        {
            return Unauthorized();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _adminUserService.SendPasswordResetAsync(
            request,
            adminUserId,
            adminEmail,
            ipAddress);

        if (!result)
        {
            return NotFound(new { message = "User not found" });
        }

        _logger.LogInformation("Password reset sent to {Email} by admin {AdminId}",
            request.Email, adminUserId);

        return Ok(new { message = "Password reset email sent successfully" });
    }

    /// <summary>
    /// Send invitation email to a new user.
    /// </summary>
    [HttpPost("invite")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendUserInvite([FromBody] AdminUserInviteRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var adminUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(adminUserId) || string.IsNullOrEmpty(adminEmail))
        {
            return Unauthorized();
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var result = await _adminUserService.SendUserInviteAsync(
            request,
            adminUserId,
            adminEmail,
            ipAddress);

        if (!result)
        {
            return BadRequest(new { message = "User with this email already exists" });
        }

        _logger.LogInformation("User invite sent to {Email} for role {Role} by admin {AdminId}",
            request.Email, request.Role, adminUserId);

        return Ok(new { message = "User invitation sent successfully" });
    }
}
