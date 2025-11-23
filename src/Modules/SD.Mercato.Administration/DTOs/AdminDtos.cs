using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Administration.DTOs;

/// <summary>
/// DTO for listing users in the admin panel.
/// </summary>
public class AdminUserListDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// DTO for detailed user information in admin panel.
/// </summary>
public class AdminUserDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? ExternalProvider { get; set; }
    public string? ExternalProviderId { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
}

/// <summary>
/// Request for searching/filtering users.
/// </summary>
public class AdminUserSearchRequest
{
    public string? SearchTerm { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsEmailVerified { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Validates and normalizes pagination parameters.
    /// Ensures PageNumber >= 1, PageSize between 1 and 100.
    /// </summary>
    public void ValidateAndNormalize()
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 1) PageSize = 20;
        if (PageSize > 100) PageSize = 100;
    }
}

/// <summary>
/// Paginated response for user lists.
/// </summary>
public class PaginatedUsersResponse
{
    public List<AdminUserListDto> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// Request to activate/deactivate a user account.
/// </summary>
public class UpdateUserStatusRequest
{
    [Required]
    public bool IsActive { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }
}

/// <summary>
/// Request to send password reset email to user.
/// </summary>
public class AdminPasswordResetRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Request to send user invitation.
/// </summary>
public class AdminUserInviteRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Message { get; set; }
}

/// <summary>
/// DTO for seller/store listing in admin panel.
/// </summary>
public class AdminStoreListDto
{
    public Guid Id { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string OwnerUserId { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ProductCount { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
}

/// <summary>
/// DTO for detailed store information in admin panel.
/// </summary>
public class AdminStoreDetailDto
{
    public Guid Id { get; set; }
    public string OwnerUserId { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
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

    // KPIs
    public int ProductCount { get; set; }
    public int TotalOrderCount { get; set; }
    public int PendingOrderCount { get; set; }
    public int CompletedOrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommissionEarned { get; set; }
}

/// <summary>
/// Request for searching/filtering stores.
/// </summary>
public class AdminStoreSearchRequest
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsVerified { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Validates and normalizes pagination parameters.
    /// Ensures PageNumber >= 1, PageSize between 1 and 100.
    /// </summary>
    public void ValidateAndNormalize()
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 1) PageSize = 20;
        if (PageSize > 100) PageSize = 100;
    }
}

/// <summary>
/// Paginated response for store lists.
/// </summary>
public class PaginatedStoresResponse
{
    public List<AdminStoreListDto> Stores { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// Request to update store status.
/// </summary>
public class UpdateStoreStatusRequest
{
    public bool? IsActive { get; set; }
    public bool? IsVerified { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }
}

/// <summary>
/// Request to update store commission rate.
/// </summary>
public class UpdateStoreCommissionRequest
{
    /// <summary>
    /// Commission rate as a percentage (0-100). For example, enter 15 for 15%.
    /// </summary>
    [Required]
    [Range(0, 100, ErrorMessage = "Commission rate must be between 0 and 100 (as a percentage)")]
    public decimal CommissionRate { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    /// <summary>
    /// Gets the commission rate as a decimal (0.0–1.0).
    /// </summary>
    public decimal CommissionRateDecimal => CommissionRate / 100m;
}

/// <summary>
/// DTO for category with admin-specific fields.
/// </summary>
public class AdminCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal? DefaultCommissionRate { get; set; }
    public int ProductCount { get; set; }
}

/// <summary>
/// Request to create a category.
/// </summary>
public class AdminCreateCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    [Range(0, 1, ErrorMessage = "Commission rate must be between 0 and 1")]
    public decimal? DefaultCommissionRate { get; set; }
}

/// <summary>
/// Request to update a category.
/// </summary>
public class AdminUpdateCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    [Range(0, 1, ErrorMessage = "Commission rate must be between 0 and 1")]
    public decimal? DefaultCommissionRate { get; set; }

    public bool? IsActive { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for audit log entries.
/// </summary>
public class AuditLogDto
{
    public Guid Id { get; set; }
    public string AdminUserId { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ChangesJson { get; set; }
    public string? IpAddress { get; set; }
    public DateTime PerformedAt { get; set; }
}

/// <summary>
/// Request for searching/filtering audit logs.
/// </summary>
public class AuditLogSearchRequest
{
    public string? AdminUserId { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? Action { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Validates and normalizes pagination parameters.
    /// Ensures PageNumber >= 1, PageSize between 1 and 100.
    /// </summary>
    public void ValidateAndNormalize()
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 1) PageSize = 50;
        if (PageSize > 100) PageSize = 100;
    }
}

/// <summary>
/// Paginated response for audit logs.
/// </summary>
public class PaginatedAuditLogsResponse
{
    public List<AuditLogDto> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
