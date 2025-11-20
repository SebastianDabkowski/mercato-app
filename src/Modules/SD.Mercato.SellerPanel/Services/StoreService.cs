using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SD.Mercato.SellerPanel.Data;
using SD.Mercato.SellerPanel.DTOs;
using SD.Mercato.SellerPanel.Models;

namespace SD.Mercato.SellerPanel.Services;

/// <summary>
/// Service for managing seller stores.
/// </summary>
public class StoreService : IStoreService
{
    private readonly SellerPanelDbContext _context;
    private readonly ILogger<StoreService> _logger;

    public StoreService(SellerPanelDbContext context, ILogger<StoreService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<StoreResponse> CreateStoreAsync(string userId, CreateStoreRequest request)
    {
        try
        {
            // Check if user already owns a store
            if (await UserOwnsStoreAsync(userId))
            {
                return new StoreResponse
                {
                    Success = false,
                    Message = "User already owns a store. Only one store per user is allowed."
                };
            }

            // Check if store name is available
            if (!await IsStoreNameAvailableAsync(request.StoreName))
            {
                return new StoreResponse
                {
                    Success = false,
                    Message = "Store name is already taken. Please choose a different name."
                };
            }

            // Validate store type
            if (request.StoreType != StoreTypes.Company && request.StoreType != StoreTypes.Individual)
            {
                return new StoreResponse
                {
                    Success = false,
                    Message = "Invalid store type. Must be 'Company' or 'Individual'."
                };
            }

            // Validate company-specific fields
            if (request.StoreType == StoreTypes.Company)
            {
                if (string.IsNullOrWhiteSpace(request.BusinessName))
                {
                    return new StoreResponse
                    {
                        Success = false,
                        Message = "Business name is required for company stores."
                    };
                }

                if (string.IsNullOrWhiteSpace(request.TaxId))
                {
                    return new StoreResponse
                    {
                        Success = false,
                        Message = "Tax ID is required for company stores."
                    };
                }
            }

            // Create store entity
            var store = new Store
            {
                Id = Guid.NewGuid(),
                OwnerUserId = userId,
                StoreName = request.StoreName.ToLowerInvariant(),
                DisplayName = request.DisplayName,
                Description = request.Description,
                LogoUrl = request.LogoUrl,
                ContactEmail = request.ContactEmail,
                PhoneNumber = request.PhoneNumber,
                StoreType = request.StoreType,
                BusinessName = request.BusinessName,
                TaxId = request.TaxId,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,
                BankAccountDetails = request.BankAccountDetails,
                DeliveryInfo = request.DeliveryInfo,
                ReturnInfo = request.ReturnInfo,
                CommissionRate = 0.15m, // Default commission rate
                IsActive = true,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Store created successfully: {StoreId} for user {UserId}", store.Id, userId);

            return new StoreResponse
            {
                Success = true,
                Message = "Store created successfully",
                Store = MapToDto(store)
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating store for user {UserId}", userId);
            return new StoreResponse
            {
                Success = false,
                Message = "A database error occurred while creating the store. Please try again."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating store for user {UserId}", userId);
            throw; // Rethrow unexpected exceptions for higher-level handling
        }
    }

    public async Task<StoreResponse> UpdateStoreProfileAsync(Guid storeId, string userId, UpdateStoreProfileRequest request)
    {
        try
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.Id == storeId && s.OwnerUserId == userId);

            if (store == null)
            {
                return new StoreResponse
                {
                    Success = false,
                    Message = "Store not found or you do not have permission to update it."
                };
            }

            // Update store properties
            store.DisplayName = request.DisplayName;
            store.Description = request.Description;
            store.LogoUrl = request.LogoUrl;
            store.ContactEmail = request.ContactEmail;
            store.PhoneNumber = request.PhoneNumber;
            store.BusinessName = request.BusinessName;
            store.TaxId = request.TaxId;
            store.AddressLine1 = request.AddressLine1;
            store.AddressLine2 = request.AddressLine2;
            store.City = request.City;
            store.State = request.State;
            store.PostalCode = request.PostalCode;
            store.Country = request.Country;
            store.BankAccountDetails = request.BankAccountDetails;
            store.DeliveryInfo = request.DeliveryInfo;
            store.ReturnInfo = request.ReturnInfo;
            store.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Store updated successfully: {StoreId}", storeId);

            return new StoreResponse
            {
                Success = true,
                Message = "Store profile updated successfully",
                Store = MapToDto(store)
            };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating store {StoreId}", storeId);
            return new StoreResponse
            {
                Success = false,
                Message = "The store profile was updated by another process. Please reload and try again."
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating store {StoreId}", storeId);
            return new StoreResponse
            {
                Success = false,
                Message = "A database error occurred while updating the store profile. Please try again."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating store {StoreId}", storeId);
            throw; // Rethrow unexpected exceptions for higher-level handling
        }
    }

    public async Task<StoreDto?> GetStoreByIdAsync(Guid storeId)
    {
        var store = await _context.Stores.FindAsync(storeId);
        return store != null ? MapToDto(store) : null;
    }

    public async Task<StoreDto?> GetStoreByOwnerIdAsync(string userId)
    {
        var store = await _context.Stores
            .FirstOrDefaultAsync(s => s.OwnerUserId == userId);
        return store != null ? MapToDto(store) : null;
    }

    public async Task<PublicStoreProfileDto?> GetPublicStoreProfileByNameAsync(string storeName)
    {
        var store = await _context.Stores
            .Where(s => s.StoreName == storeName.ToLowerInvariant() && s.IsActive)
            .FirstOrDefaultAsync();

        if (store == null)
        {
            return null;
        }

        return new PublicStoreProfileDto
        {
            StoreName = store.StoreName,
            DisplayName = store.DisplayName,
            Description = store.Description,
            LogoUrl = store.LogoUrl,
            DeliveryInfo = store.DeliveryInfo,
            ReturnInfo = store.ReturnInfo,
            CreatedAt = store.CreatedAt
        };
    }

    public async Task<bool> IsStoreNameAvailableAsync(string storeName)
    {
        return !await _context.Stores
            .AnyAsync(s => s.StoreName == storeName.ToLowerInvariant());
    }

    public async Task<bool> UserOwnsStoreAsync(string userId)
    {
        return await _context.Stores
            .AnyAsync(s => s.OwnerUserId == userId);
    }

    private static StoreDto MapToDto(Store store)
    {
        return new StoreDto
        {
            Id = store.Id,
            OwnerUserId = store.OwnerUserId,
            StoreName = store.StoreName,
            DisplayName = store.DisplayName,
            Description = store.Description,
            LogoUrl = store.LogoUrl,
            ContactEmail = store.ContactEmail,
            PhoneNumber = store.PhoneNumber,
            StoreType = store.StoreType,
            BusinessName = store.BusinessName,
            TaxId = store.TaxId,
            AddressLine1 = store.AddressLine1,
            AddressLine2 = store.AddressLine2,
            City = store.City,
            State = store.State,
            PostalCode = store.PostalCode,
            Country = store.Country,
            CommissionRate = store.CommissionRate,
            IsActive = store.IsActive,
            IsVerified = store.IsVerified,
            CreatedAt = store.CreatedAt,
            UpdatedAt = store.UpdatedAt,
            DeliveryInfo = store.DeliveryInfo,
            ReturnInfo = store.ReturnInfo
        };
    }
}
