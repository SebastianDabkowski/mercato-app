using SD.Mercato.Users.DTOs;

namespace SD.Mercato.Users.Services;

/// <summary>
/// Interface for GDPR compliance services.
/// </summary>
public interface IGdprService
{
    /// <summary>
    /// Update user's email marketing consent.
    /// </summary>
    Task<ConsentResponse> UpdateEmailMarketingConsentAsync(string userId, UpdateConsentRequest request);

    /// <summary>
    /// Get user's current consent settings.
    /// </summary>
    Task<ConsentResponse?> GetConsentAsync(string userId);

    /// <summary>
    /// Export all user data in the specified format (JSON or CSV).
    /// GDPR Right to Data Portability (Article 20).
    /// </summary>
    Task<DataExportResponse?> ExportUserDataAsync(string userId, DataExportRequest request);

    /// <summary>
    /// Delete user account and anonymize personal data.
    /// GDPR Right to Erasure (Article 17).
    /// Anonymizes user data while retaining records for legal/accounting requirements.
    /// </summary>
    Task<DeleteAccountResponse> DeleteUserAccountAsync(string userId, DeleteAccountRequest request);
}
