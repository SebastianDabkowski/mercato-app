using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SD.Mercato.Administration.DTOs;
using SD.Mercato.Users.Models;
using SD.Mercato.Users.Data;

namespace SD.Mercato.Administration.Services;

/// <summary>
/// Service for admin GDPR compliance operations.
/// </summary>
public class AdminGdprService : IAdminGdprService
{
    private readonly UsersDbContext _usersContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminGdprService> _logger;

    public AdminGdprService(
        UsersDbContext usersContext,
        UserManager<ApplicationUser> userManager,
        ILogger<AdminGdprService> logger)
    {
        _usersContext = usersContext;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<AdminGdprUserDataResponse?> GetUserDataAsync(AdminGdprSearchRequest request)
    {
        // Find user by ID or email
        ApplicationUser? user = null;

        if (!string.IsNullOrEmpty(request.UserId))
        {
            user = await _userManager.FindByIdAsync(request.UserId);
        }
        else if (!string.IsNullOrEmpty(request.Email))
        {
            user = await _userManager.FindByEmailAsync(request.Email);
        }
        else
        {
            return null;
        }

        if (user == null)
        {
            return null;
        }

        // Get user role
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Unknown";

        var response = new AdminGdprUserDataResponse
        {
            User = new UserGdprData
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = role,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsDeleted = user.IsDeleted,
                DeletedAt = user.DeletedAt,
                IsEmailVerified = user.IsEmailVerified,
                ExternalProvider = user.ExternalProvider
            },
            Consent = new ConsentGdprData
            {
                EmailMarketingConsent = user.EmailMarketingConsent,
                EmailMarketingConsentUpdatedAt = user.EmailMarketingConsentUpdatedAt
            },
            Orders = await GetUserOrdersAsync(user.Id),
            Notifications = await GetUserNotificationsAsync(user.Id)
        };

        _logger.LogInformation(
            "Admin retrieved GDPR data for user {UserId} ({Email})",
            user.Id,
            user.Email);

        return response;
    }

    /// <summary>
    /// Get user orders for GDPR audit.
    /// TODO: This requires querying the History module.
    /// </summary>
    private async Task<List<OrderGdprData>> GetUserOrdersAsync(string userId)
    {
        // TODO: Query History module for orders
        // For now, return empty list
        return await Task.FromResult(new List<OrderGdprData>());
    }

    /// <summary>
    /// Get user notifications for GDPR audit.
    /// TODO: This requires querying the Notification module.
    /// </summary>
    private async Task<List<NotificationGdprData>> GetUserNotificationsAsync(string userId)
    {
        // TODO: Query Notification module for notifications
        // For now, return empty list
        return await Task.FromResult(new List<NotificationGdprData>());
    }
}
