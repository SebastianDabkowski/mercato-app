using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SD.Mercato.Administration.DTOs;
using SD.Mercato.Administration.Models;
using SD.Mercato.Users.Models;
using System.Text.Json;

namespace SD.Mercato.Administration.Services;

/// <summary>
/// Service for admin user management operations.
/// </summary>
public class AdminUserService : IAdminUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogService _auditLogService;

    public AdminUserService(
        UserManager<ApplicationUser> userManager,
        IAuditLogService auditLogService)
    {
        _userManager = userManager;
        _auditLogService = auditLogService;
    }

    public async Task<PaginatedUsersResponse> SearchUsersAsync(AdminUserSearchRequest request)
    {
        // Validate pagination parameters
        request.ValidateAndNormalize();

        var query = _userManager.Users.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(u =>
                u.Email!.ToLower().Contains(searchLower) ||
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower));
        }

        // Apply role filter
        if (!string.IsNullOrEmpty(request.Role))
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(request.Role);
            var userIds = usersInRole.Select(u => u.Id).ToHashSet();
            query = query.Where(u => userIds.Contains(u.Id));
        }

        // Apply email verified filter
        if (request.IsEmailVerified.HasValue)
        {
            query = query.Where(u => u.IsEmailVerified == request.IsEmailVerified.Value);
        }

        // TODO: Add IsActive filter when user activation/deactivation is implemented
        // Currently using LockoutEnabled as a proxy for active status
        if (request.IsActive.HasValue)
        {
            query = query.Where(u =>
                request.IsActive.Value
                    ? (!u.LockoutEnabled || u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow)
                    : (u.LockoutEnabled && u.LockoutEnd > DateTimeOffset.UtcNow)
            );
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // Map to DTOs with roles
        var userDtos = new List<AdminUserListDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var isActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;

            userDtos.Add(new AdminUserListDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? "None",
                IsActive = isActive,
                IsEmailVerified = user.IsEmailVerified,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            });
        }

        return new PaginatedUsersResponse
        {
            Users = userDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<AdminUserDetailDto?> GetUserDetailAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var isActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;

        return new AdminUserDetailDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role = roles.FirstOrDefault() ?? "None",
            IsActive = isActive,
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            ExternalProvider = user.ExternalProvider,
            ExternalProviderId = user.ExternalProviderId,
            LockoutEnabled = user.LockoutEnabled,
            LockoutEnd = user.LockoutEnd,
            AccessFailedCount = user.AccessFailedCount
        };
    }

    public async Task<bool> UpdateUserStatusAsync(
        string userId,
        UpdateUserStatusRequest request,
        string adminUserId,
        string adminEmail,
        string? ipAddress)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var oldStatus = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow;

        // Update lockout status to activate/deactivate user
        if (request.IsActive)
        {
            // Activate user by removing lockout
            user.LockoutEnd = null;
            user.LockoutEnabled = false;
        }
        else
        {
            // Deactivate user by setting lockout indefinitely
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;
        }

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            // Log the action
            var changes = new
            {
                OldStatus = oldStatus ? "Active" : "Inactive",
                NewStatus = request.IsActive ? "Active" : "Inactive",
                Reason = request.Reason
            };

            await _auditLogService.LogActionAsync(
                adminUserId,
                adminEmail,
                request.IsActive ? AuditActions.UserActivated : AuditActions.UserDeactivated,
                EntityTypes.User,
                userId,
                $"User {user.Email} {(request.IsActive ? "activated" : "deactivated")} by admin. Reason: {request.Reason ?? "Not specified"}",
                JsonSerializer.Serialize(changes),
                ipAddress);

            return true;
        }

        return false;
    }

    public async Task<bool> SendPasswordResetAsync(
        AdminPasswordResetRequest request,
        string adminUserId,
        string adminEmail,
        string? ipAddress)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return false;
        }

        // TODO: Implement actual password reset email sending
        // For now, just generate a reset token (will be used when email sending is implemented)
        // await _userManager.GeneratePasswordResetTokenAsync(user);

        // Log the action
        await _auditLogService.LogActionAsync(
            adminUserId,
            adminEmail,
            AuditActions.UserPasswordReset,
            EntityTypes.User,
            user.Id,
            $"Password reset initiated for user {user.Email} by admin",
            null,
            ipAddress);

        // TODO: Send email with reset token
        return true;
    }

    public async Task<bool> SendUserInviteAsync(
        AdminUserInviteRequest request,
        string adminUserId,
        string adminEmail,
        string? ipAddress)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return false;
        }

        // Log the action
        await _auditLogService.LogActionAsync(
            adminUserId,
            adminEmail,
            AuditActions.UserInviteSent,
            EntityTypes.User,
            request.Email,
            $"Invitation sent to {request.Email} for role {request.Role} by admin. Message: {request.Message ?? "None"}",
            null,
            ipAddress);

        // TODO: Implement actual invitation email sending
        // This would typically create a temporary invite record and send an email
        return true;
    }
}
