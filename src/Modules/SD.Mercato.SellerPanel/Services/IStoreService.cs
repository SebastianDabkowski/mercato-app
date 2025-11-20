using SD.Mercato.SellerPanel.DTOs;

namespace SD.Mercato.SellerPanel.Services;

/// <summary>
/// Interface for store management services.
/// </summary>
public interface IStoreService
{
    /// <summary>
    /// Create a new store for a seller.
    /// </summary>
    Task<StoreResponse> CreateStoreAsync(string userId, CreateStoreRequest request);

    /// <summary>
    /// Update an existing store profile.
    /// </summary>
    Task<StoreResponse> UpdateStoreProfileAsync(Guid storeId, string userId, UpdateStoreProfileRequest request);

    /// <summary>
    /// Get a store by ID (for authenticated seller).
    /// </summary>
    Task<StoreDto?> GetStoreByIdAsync(Guid storeId);

    /// <summary>
    /// Get a store by owner user ID.
    /// </summary>
    Task<StoreDto?> GetStoreByOwnerIdAsync(string userId);

    /// <summary>
    /// Get a public store profile by store name.
    /// </summary>
    Task<PublicStoreProfileDto?> GetPublicStoreProfileByNameAsync(string storeName);

    /// <summary>
    /// Get all active stores (for filters).
    /// </summary>
    Task<List<StoreListItemDto>> GetActiveStoresAsync();

    /// <summary>
    /// Check if a store name is available.
    /// </summary>
    Task<bool> IsStoreNameAvailableAsync(string storeName);

    /// <summary>
    /// Check if a user already owns a store.
    /// </summary>
    Task<bool> UserOwnsStoreAsync(string userId);
}
