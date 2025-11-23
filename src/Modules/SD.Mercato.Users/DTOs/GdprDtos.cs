using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Users.DTOs;

/// <summary>
/// Request to update email marketing consent.
/// </summary>
public class UpdateConsentRequest
{
    [Required]
    public bool EmailMarketingConsent { get; set; }
}

/// <summary>
/// Response containing user consent information.
/// </summary>
public class ConsentResponse
{
    public bool EmailMarketingConsent { get; set; }
    public DateTime? EmailMarketingConsentUpdatedAt { get; set; }
}

/// <summary>
/// Request to export user data (GDPR data portability).
/// </summary>
public class DataExportRequest
{
    /// <summary>
    /// Format for the export: "json" or "csv"
    /// </summary>
    [Required]
    public string Format { get; set; } = "json";
}

/// <summary>
/// Response containing exported user data.
/// </summary>
public class DataExportResponse
{
    public UserDataExport Data { get; set; } = new();
    public string Format { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Complete user data export for GDPR compliance.
/// </summary>
public class UserDataExport
{
    public UserProfileData Profile { get; set; } = new();
    public List<OrderData> Orders { get; set; } = new();
    public ConsentData Consent { get; set; } = new();
    public AccountData Account { get; set; } = new();
}

/// <summary>
/// User profile data for export.
/// </summary>
public class UserProfileData
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Order data for export (buyer perspective).
/// </summary>
public class OrderData
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public AddressData DeliveryAddress { get; set; } = new();
    public List<OrderItemData> Items { get; set; } = new();
}

/// <summary>
/// Address data for export.
/// </summary>
public class AddressData
{
    public string RecipientName { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

/// <summary>
/// Order item data for export.
/// </summary>
public class OrderItemData
{
    public string ProductName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal { get; set; }
}

/// <summary>
/// Consent data for export.
/// </summary>
public class ConsentData
{
    public bool EmailMarketingConsent { get; set; }
    public DateTime? EmailMarketingConsentUpdatedAt { get; set; }
}

/// <summary>
/// Account data for export.
/// </summary>
public class AccountData
{
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? ExternalProvider { get; set; }
}

/// <summary>
/// Request to delete user account (GDPR right to erasure).
/// </summary>
public class DeleteAccountRequest
{
    /// <summary>
    /// User must confirm deletion by typing their email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public string ConfirmationEmail { get; set; } = string.Empty;

    /// <summary>
    /// Optional reason for account deletion.
    /// </summary>
    [MaxLength(1000)]
    public string? Reason { get; set; }
}

/// <summary>
/// Response for account deletion request.
/// </summary>
public class DeleteAccountResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
}
