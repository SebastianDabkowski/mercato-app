using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SD.Mercato.Users.Data;
using SD.Mercato.Users.DTOs;
using SD.Mercato.Users.Models;
using System.Text;
using System.Text.Json;

namespace SD.Mercato.Users.Services;

/// <summary>
/// Service for GDPR compliance operations.
/// </summary>
public class GdprService : IGdprService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UsersDbContext _context;
    private readonly ILogger<GdprService> _logger;

    public GdprService(
        UserManager<ApplicationUser> userManager,
        UsersDbContext context,
        ILogger<GdprService> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    public async Task<ConsentResponse> UpdateEmailMarketingConsentAsync(string userId, UpdateConsentRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.IsDeleted)
        {
            throw new InvalidOperationException("Cannot update consent for deleted account");
        }

        user.EmailMarketingConsent = request.EmailMarketingConsent;
        user.EmailMarketingConsentUpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Failed to update consent");
        }

        _logger.LogInformation(
            "User {UserId} updated email marketing consent to {Consent}",
            userId,
            request.EmailMarketingConsent);

        return new ConsentResponse
        {
            EmailMarketingConsent = user.EmailMarketingConsent,
            EmailMarketingConsentUpdatedAt = user.EmailMarketingConsentUpdatedAt
        };
    }

    public async Task<ConsentResponse?> GetConsentAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        return new ConsentResponse
        {
            EmailMarketingConsent = user.EmailMarketingConsent,
            EmailMarketingConsentUpdatedAt = user.EmailMarketingConsentUpdatedAt
        };
    }

    public async Task<DataExportResponse?> ExportUserDataAsync(string userId, DataExportRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        if (user.IsDeleted)
        {
            return null;
        }

        // Build the data export
        var exportData = new UserDataExport
        {
            Profile = new UserProfileData
            {
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt
            },
            Consent = new ConsentData
            {
                EmailMarketingConsent = user.EmailMarketingConsent,
                EmailMarketingConsentUpdatedAt = user.EmailMarketingConsentUpdatedAt
            },
            Account = new AccountData
            {
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsEmailVerified = user.IsEmailVerified,
                ExternalProvider = user.ExternalProvider
            },
            Orders = await GetUserOrdersForExportAsync(userId)
        };

        _logger.LogInformation(
            "User {UserId} exported their data in {Format} format",
            userId,
            request.Format);

        return new DataExportResponse
        {
            Data = exportData,
            Format = request.Format.ToLowerInvariant(),
            ExportedAt = DateTime.UtcNow
        };
    }

    public async Task<DeleteAccountResponse> DeleteUserAccountAsync(string userId, DeleteAccountRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new DeleteAccountResponse
            {
                Success = false,
                Message = "User not found"
            };
        }

        if (user.IsDeleted)
        {
            return new DeleteAccountResponse
            {
                Success = false,
                Message = "Account is already deleted"
            };
        }

        // Verify confirmation email matches
        if (!string.Equals(user.Email, request.ConfirmationEmail, StringComparison.OrdinalIgnoreCase))
        {
            return new DeleteAccountResponse
            {
                Success = false,
                Message = "Confirmation email does not match"
            };
        }

        // Mark account as deleted
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        // Anonymize personal data (GDPR Right to Erasure)
        // We keep the user record for legal/accounting purposes but remove PII
        user.FirstName = "[DELETED]";
        user.LastName = "[DELETED]";
        user.Email = $"deleted_{userId}@anonymized.mercato";
        user.NormalizedEmail = user.Email.ToUpperInvariant();
        user.UserName = $"deleted_{userId}";
        user.NormalizedUserName = user.UserName.ToUpperInvariant();
        user.PhoneNumber = null;
        user.EmailMarketingConsent = false;

        // Clear password hash for security
        // Note: RemovePasswordAsync already clears the hash, no need to add a new one
        await _userManager.RemovePasswordAsync(user);

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return new DeleteAccountResponse
            {
                Success = false,
                Message = "Failed to delete account"
            };
        }

        _logger.LogInformation(
            "User {UserId} account deleted and anonymized. Reason: {Reason}",
            userId,
            request.Reason ?? "Not specified");

        return new DeleteAccountResponse
        {
            Success = true,
            Message = "Account successfully deleted. Personal data has been anonymized.",
            DeletedAt = user.DeletedAt
        };
    }

    /// <summary>
    /// Retrieves user order data for export asynchronously.
    /// Note: This method needs to query the History module, which we'll integrate later.
    /// For now, it returns an empty list as a placeholder.
    /// </summary>
    private async Task<List<OrderData>> GetUserOrdersForExportAsync(string userId)
    {
        // TODO: Query History module for user orders using async operations
        // Example: return await _historyContext.Orders.Where(...).ToListAsync();
        await Task.CompletedTask; // Placeholder to make method truly async
        return new List<OrderData>();
    }
}
