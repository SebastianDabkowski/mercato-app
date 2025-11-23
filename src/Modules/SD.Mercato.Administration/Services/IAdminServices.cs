using SD.Mercato.Administration.DTOs;

namespace SD.Mercato.Administration.Services;

/// <summary>
/// Interface for admin user management services.
/// </summary>
public interface IAdminUserService
{
    /// <summary>
    /// Search and list users with filtering and pagination.
    /// </summary>
    Task<PaginatedUsersResponse> SearchUsersAsync(AdminUserSearchRequest request);

    /// <summary>
    /// Get detailed user information.
    /// </summary>
    Task<AdminUserDetailDto?> GetUserDetailAsync(string userId);

    /// <summary>
    /// Activate or deactivate a user account.
    /// </summary>
    Task<bool> UpdateUserStatusAsync(string userId, UpdateUserStatusRequest request, string adminUserId, string adminEmail, string? ipAddress);

    /// <summary>
    /// Send password reset email to a user.
    /// </summary>
    Task<bool> SendPasswordResetAsync(AdminPasswordResetRequest request, string adminUserId, string adminEmail, string? ipAddress);

    /// <summary>
    /// Send invitation email to a new user.
    /// </summary>
    Task<bool> SendUserInviteAsync(AdminUserInviteRequest request, string adminUserId, string adminEmail, string? ipAddress);
}

/// <summary>
/// Interface for admin seller/store management services.
/// </summary>
public interface IAdminStoreService
{
    /// <summary>
    /// Search and list stores with filtering and pagination.
    /// </summary>
    Task<PaginatedStoresResponse> SearchStoresAsync(AdminStoreSearchRequest request);

    /// <summary>
    /// Get detailed store information with KPIs.
    /// </summary>
    Task<AdminStoreDetailDto?> GetStoreDetailAsync(Guid storeId);

    /// <summary>
    /// Update store status (active/verified).
    /// </summary>
    Task<bool> UpdateStoreStatusAsync(Guid storeId, UpdateStoreStatusRequest request, string adminUserId, string adminEmail, string? ipAddress);

    /// <summary>
    /// Update store commission rate.
    /// </summary>
    Task<bool> UpdateStoreCommissionAsync(Guid storeId, UpdateStoreCommissionRequest request, string adminUserId, string adminEmail, string? ipAddress);
}

/// <summary>
/// Interface for admin category management services.
/// </summary>
public interface IAdminCategoryService
{
    /// <summary>
    /// Get all categories with admin-specific information.
    /// </summary>
    Task<List<AdminCategoryDto>> GetAllCategoriesAsync();

    /// <summary>
    /// Get category by ID with admin-specific information.
    /// </summary>
    Task<AdminCategoryDto?> GetCategoryByIdAsync(Guid categoryId);

    /// <summary>
    /// Create a new category.
    /// </summary>
    Task<AdminCategoryDto> CreateCategoryAsync(AdminCreateCategoryRequest request, string adminUserId, string adminEmail, string? ipAddress);

    /// <summary>
    /// Update an existing category.
    /// </summary>
    Task<AdminCategoryDto?> UpdateCategoryAsync(Guid categoryId, AdminUpdateCategoryRequest request, string adminUserId, string adminEmail, string? ipAddress);

    /// <summary>
    /// Delete a category (soft delete).
    /// </summary>
    Task<bool> DeleteCategoryAsync(Guid categoryId, string adminUserId, string adminEmail, string? ipAddress);
}

/// <summary>
/// Interface for audit logging services.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Log an admin action.
    /// </summary>
    Task LogActionAsync(
        string adminUserId,
        string adminEmail,
        string action,
        string entityType,
        string entityId,
        string description,
        string? changesJson = null,
        string? ipAddress = null);

    /// <summary>
    /// Search and retrieve audit logs with filtering and pagination.
    /// </summary>
    Task<PaginatedAuditLogsResponse> SearchAuditLogsAsync(AuditLogSearchRequest request);

    /// <summary>
    /// Get audit logs for a specific entity.
    /// </summary>
    Task<List<AuditLogDto>> GetEntityAuditLogsAsync(string entityType, string entityId);
}
