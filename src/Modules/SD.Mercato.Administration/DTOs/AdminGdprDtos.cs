using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Administration.DTOs;

/// <summary>
/// Request to search for user data for GDPR compliance.
/// </summary>
public class AdminGdprSearchRequest
{
    /// <summary>
    /// User ID to search for.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Email address to search for.
    /// </summary>
    [EmailAddress]
    public string? Email { get; set; }
}

/// <summary>
/// Response containing all user data for GDPR compliance audit.
/// </summary>
public class AdminGdprUserDataResponse
{
    public UserGdprData User { get; set; } = new();
    public List<OrderGdprData> Orders { get; set; } = new();
    public List<NotificationGdprData> Notifications { get; set; } = new();
    public ConsentGdprData Consent { get; set; } = new();
}

/// <summary>
/// User data for GDPR audit.
/// </summary>
public class UserGdprData
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? ExternalProvider { get; set; }
}

/// <summary>
/// Order data for GDPR audit.
/// </summary>
public class OrderGdprData
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string BuyerPhone { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
}

/// <summary>
/// Notification data for GDPR audit.
/// </summary>
public class NotificationGdprData
{
    public Guid Id { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? RecipientEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Consent data for GDPR audit.
/// </summary>
public class ConsentGdprData
{
    public bool EmailMarketingConsent { get; set; }
    public DateTime? EmailMarketingConsentUpdatedAt { get; set; }
}
