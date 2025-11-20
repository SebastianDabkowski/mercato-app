using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.SellerPanel.DTOs;

/// <summary>
/// Request model for creating a new store during seller onboarding.
/// </summary>
public class CreateStoreRequest
{
    [Required(ErrorMessage = "Store name is required")]
    [MaxLength(100, ErrorMessage = "Store name cannot exceed 100 characters")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Store name can only contain lowercase letters, numbers, and hyphens")]
    public string StoreName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Display name is required")]
    [MaxLength(200, ErrorMessage = "Display name cannot exceed 200 characters")]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    [Url(ErrorMessage = "Invalid URL format")]
    public string? LogoUrl { get; set; }

    [Required(ErrorMessage = "Contact email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string ContactEmail { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Store type is required")]
    public string StoreType { get; set; } = string.Empty;

    // Company-specific fields
    public string? BusinessName { get; set; }
    public string? TaxId { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    public string? BankAccountDetails { get; set; }
    public string? DeliveryInfo { get; set; }
    public string? ReturnInfo { get; set; }
}

/// <summary>
/// Request model for updating store profile.
/// </summary>
public class UpdateStoreProfileRequest
{
    [Required(ErrorMessage = "Display name is required")]
    [MaxLength(200, ErrorMessage = "Display name cannot exceed 200 characters")]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    [Url(ErrorMessage = "Invalid URL format")]
    public string? LogoUrl { get; set; }

    [Required(ErrorMessage = "Contact email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string ContactEmail { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }

    // Company-specific fields (can be updated)
    public string? BusinessName { get; set; }
    public string? TaxId { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    public string? BankAccountDetails { get; set; }
    public string? DeliveryInfo { get; set; }
    public string? ReturnInfo { get; set; }
}

/// <summary>
/// Response model for store operations.
/// </summary>
public class StoreResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public StoreDto? Store { get; set; }
}

/// <summary>
/// Store data transfer object.
/// </summary>
public class StoreDto
{
    public Guid Id { get; set; }
    public string OwnerUserId { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string StoreType { get; set; } = string.Empty;
    public string? BusinessName { get; set; }
    public string? TaxId { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public decimal CommissionRate { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? DeliveryInfo { get; set; }
    public string? ReturnInfo { get; set; }
}

/// <summary>
/// Public store profile DTO (for buyer-facing store page).
/// </summary>
public class PublicStoreProfileDto
{
    public string StoreName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? DeliveryInfo { get; set; }
    public string? ReturnInfo { get; set; }
    public DateTime CreatedAt { get; set; }
    // TODO: Add rating summary when review system is implemented
    // public decimal? AverageRating { get; set; }
    // public int TotalReviews { get; set; }
}

/// <summary>
/// Lightweight store DTO for listing purposes (filters, dropdowns).
/// </summary>
public class StoreListItemDto
{
    public Guid Id { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
