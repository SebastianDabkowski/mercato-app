using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.SellerPanel.Models;

/// <summary>
/// Represents a seller's store on the Mercato platform.
/// </summary>
public class Store
{
    /// <summary>
    /// Unique identifier for the store.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// User ID of the store owner (foreign key to ApplicationUser in Users module).
    /// </summary>
    [Required]
    public string OwnerUserId { get; set; } = string.Empty;

    /// <summary>
    /// Unique store name used in URLs (e.g., mercato.pl/store-name).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the store (can be different from StoreName).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Store description visible on the public store page.
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// URL or path to the store logo image.
    /// </summary>
    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Contact email for the store.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>
    /// Contact phone number for the store.
    /// </summary>
    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Store type: "Company" or "Individual".
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string StoreType { get; set; } = string.Empty;

    /// <summary>
    /// Business/company name (required for company stores).
    /// </summary>
    [MaxLength(200)]
    public string? BusinessName { get; set; }

    /// <summary>
    /// Tax identification number (required for company stores).
    /// </summary>
    [MaxLength(50)]
    public string? TaxId { get; set; }

    /// <summary>
    /// Business address line 1.
    /// </summary>
    [MaxLength(200)]
    public string? AddressLine1 { get; set; }

    /// <summary>
    /// Business address line 2.
    /// </summary>
    [MaxLength(200)]
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// City.
    /// </summary>
    [MaxLength(100)]
    public string? City { get; set; }

    /// <summary>
    /// State or province.
    /// </summary>
    [MaxLength(100)]
    public string? State { get; set; }

    /// <summary>
    /// Postal code.
    /// </summary>
    [MaxLength(20)]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Country.
    /// </summary>
    [MaxLength(100)]
    public string? Country { get; set; }

    /// <summary>
    /// Bank account details for payouts (encrypted/secure storage recommended).
    /// </summary>
    // TODO: Should bank account details be stored in a separate secure table with encryption?
    [MaxLength(500)]
    public string? BankAccountDetails { get; set; }

    /// <summary>
    /// Platform commission rate for this store (e.g., 0.15 for 15%).
    /// </summary>
    [Required]
    public decimal CommissionRate { get; set; } = 0.15m;

    /// <summary>
    /// Indicates if the store is active and visible on the platform.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates if the store owner's identity or business has been verified.
    /// </summary>
    [Required]
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// Timestamp when the store was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the store was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Additional information about delivery policies.
    /// </summary>
    [MaxLength(1000)]
    public string? DeliveryInfo { get; set; }

    /// <summary>
    /// Additional information about return policies.
    /// </summary>
    [MaxLength(1000)]
    public string? ReturnInfo { get; set; }
}

/// <summary>
/// Store type constants.
/// </summary>
public static class StoreTypes
{
    public const string Company = "Company";
    public const string Individual = "Individual";
}
