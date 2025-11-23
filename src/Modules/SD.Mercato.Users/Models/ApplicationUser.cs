using Microsoft.AspNetCore.Identity;

namespace SD.Mercato.Users.Models;

/// <summary>
/// Represents a user in the Mercato system.
/// Extends IdentityUser to integrate with ASP.NET Core Identity.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// First name of the user.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name of the user.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the email has been verified.
    /// </summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>
    /// Timestamp of when the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp of the last login.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// External login provider (e.g., "Google", "Facebook", or null for email/password).
    /// </summary>
    public string? ExternalProvider { get; set; }

    /// <summary>
    /// External provider user ID.
    /// </summary>
    public string? ExternalProviderId { get; set; }

    /// <summary>
    /// GDPR: Indicates whether the user has consented to receive marketing emails.
    /// </summary>
    public bool EmailMarketingConsent { get; set; }

    /// <summary>
    /// GDPR: Timestamp when email marketing consent was last updated.
    /// </summary>
    public DateTime? EmailMarketingConsentUpdatedAt { get; set; }

    /// <summary>
    /// GDPR: Indicates if the user account has been deleted (soft delete for GDPR compliance).
    /// When true, personal data is anonymized but the record is retained for legal/accounting purposes.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// GDPR: Timestamp when the account was deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
