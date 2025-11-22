using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SD.Mercato.Administration.DTOs;
using SD.Mercato.Administration.Models;
using SD.Mercato.SellerPanel.Data;
using SD.Mercato.SellerPanel.Models;
using SD.Mercato.ProductCatalog.Data;
using SD.Mercato.History.Data;
using SD.Mercato.Users.Models;
using System.Text.Json;

namespace SD.Mercato.Administration.Services;

/// <summary>
/// Service for admin store/seller management operations.
/// </summary>
public class AdminStoreService : IAdminStoreService
{
    private readonly SellerPanelDbContext _sellerContext;
    private readonly ProductCatalogDbContext _catalogContext;
    private readonly HistoryDbContext _historyContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditLogService _auditLogService;

    public AdminStoreService(
        SellerPanelDbContext sellerContext,
        ProductCatalogDbContext catalogContext,
        HistoryDbContext historyContext,
        UserManager<ApplicationUser> userManager,
        IAuditLogService auditLogService)
    {
        _sellerContext = sellerContext;
        _catalogContext = catalogContext;
        _historyContext = historyContext;
        _userManager = userManager;
        _auditLogService = auditLogService;
    }

    public async Task<PaginatedStoresResponse> SearchStoresAsync(AdminStoreSearchRequest request)
    {
        var query = _sellerContext.Stores.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(s =>
                s.StoreName.ToLower().Contains(searchLower) ||
                s.DisplayName.ToLower().Contains(searchLower) ||
                s.ContactEmail.ToLower().Contains(searchLower));
        }

        // Apply active filter
        if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        }

        // Apply verified filter
        if (request.IsVerified.HasValue)
        {
            query = query.Where(s => s.IsVerified == request.IsVerified.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var stores = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // Map to DTOs with KPIs
        var storeDtos = new List<AdminStoreListDto>();
        foreach (var store in stores)
        {
            var owner = await _userManager.FindByIdAsync(store.OwnerUserId);
            var productCount = await _catalogContext.Products.CountAsync(p => p.StoreId == store.Id);
            var orderCount = await _historyContext.SubOrders.CountAsync(so => so.StoreId == store.Id);
            var totalRevenue = await _historyContext.SubOrders
                .Where(so => so.StoreId == store.Id && so.Status == "Completed")
                .SumAsync(so => so.TotalAmount);

            storeDtos.Add(new AdminStoreListDto
            {
                Id = store.Id,
                StoreName = store.StoreName,
                DisplayName = store.DisplayName,
                OwnerUserId = store.OwnerUserId,
                OwnerEmail = owner?.Email ?? "Unknown",
                OwnerName = owner != null ? $"{owner.FirstName} {owner.LastName}" : "Unknown",
                IsActive = store.IsActive,
                IsVerified = store.IsVerified,
                CreatedAt = store.CreatedAt,
                ProductCount = productCount,
                OrderCount = orderCount,
                TotalRevenue = totalRevenue
            });
        }

        return new PaginatedStoresResponse
        {
            Stores = storeDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<AdminStoreDetailDto?> GetStoreDetailAsync(Guid storeId)
    {
        var store = await _sellerContext.Stores.FindAsync(storeId);
        if (store == null)
        {
            return null;
        }

        var owner = await _userManager.FindByIdAsync(store.OwnerUserId);
        var productCount = await _catalogContext.Products.CountAsync(p => p.StoreId == storeId);
        
        var totalOrderCount = await _historyContext.SubOrders.CountAsync(so => so.StoreId == storeId);
        var pendingOrderCount = await _historyContext.SubOrders.CountAsync(so => so.StoreId == storeId && so.Status == "Pending");
        var completedOrderCount = await _historyContext.SubOrders.CountAsync(so => so.StoreId == storeId && so.Status == "Completed");
        
        var totalRevenue = await _historyContext.SubOrders
            .Where(so => so.StoreId == storeId && so.Status == "Completed")
            .SumAsync(so => so.TotalAmount);

        var totalCommissionEarned = totalRevenue * store.CommissionRate;

        return new AdminStoreDetailDto
        {
            Id = store.Id,
            OwnerUserId = store.OwnerUserId,
            OwnerEmail = owner?.Email ?? "Unknown",
            OwnerName = owner != null ? $"{owner.FirstName} {owner.LastName}" : "Unknown",
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
            ProductCount = productCount,
            TotalOrderCount = totalOrderCount,
            PendingOrderCount = pendingOrderCount,
            CompletedOrderCount = completedOrderCount,
            TotalRevenue = totalRevenue,
            TotalCommissionEarned = totalCommissionEarned
        };
    }

    public async Task<bool> UpdateStoreStatusAsync(
        Guid storeId,
        UpdateStoreStatusRequest request,
        string adminUserId,
        string adminEmail,
        string? ipAddress)
    {
        var store = await _sellerContext.Stores.FindAsync(storeId);
        if (store == null)
        {
            return false;
        }

        var changes = new Dictionary<string, object>();

        if (request.IsActive.HasValue && store.IsActive != request.IsActive.Value)
        {
            changes["IsActive"] = new { Old = store.IsActive, New = request.IsActive.Value };
            store.IsActive = request.IsActive.Value;
        }

        if (request.IsVerified.HasValue && store.IsVerified != request.IsVerified.Value)
        {
            changes["IsVerified"] = new { Old = store.IsVerified, New = request.IsVerified.Value };
            store.IsVerified = request.IsVerified.Value;
        }

        if (changes.Count == 0)
        {
            return false; // No changes made
        }

        store.UpdatedAt = DateTime.UtcNow;
        await _sellerContext.SaveChangesAsync();

        // Log the action
        var action = request.IsActive.HasValue && request.IsActive.Value
            ? AuditActions.StoreActivated
            : request.IsActive.HasValue && !request.IsActive.Value
            ? AuditActions.StoreDeactivated
            : request.IsVerified.HasValue && request.IsVerified.Value
            ? AuditActions.StoreVerified
            : "StoreStatusUpdated";

        var description = $"Store {store.StoreName} status updated by admin. " +
                         (request.IsActive.HasValue ? $"Active: {request.IsActive.Value}. " : "") +
                         (request.IsVerified.HasValue ? $"Verified: {request.IsVerified.Value}. " : "") +
                         $"Reason: {request.Reason ?? "Not specified"}";

        await _auditLogService.LogActionAsync(
            adminUserId,
            adminEmail,
            action,
            EntityTypes.Store,
            storeId.ToString(),
            description,
            JsonSerializer.Serialize(new { Changes = changes, Reason = request.Reason }),
            ipAddress);

        return true;
    }

    public async Task<bool> UpdateStoreCommissionAsync(
        Guid storeId,
        UpdateStoreCommissionRequest request,
        string adminUserId,
        string adminEmail,
        string? ipAddress)
    {
        var store = await _sellerContext.Stores.FindAsync(storeId);
        if (store == null)
        {
            return false;
        }

        var oldCommissionRate = store.CommissionRate;
        store.CommissionRate = request.CommissionRate;
        store.UpdatedAt = DateTime.UtcNow;

        await _sellerContext.SaveChangesAsync();

        // Log the action
        var changes = new
        {
            OldCommissionRate = oldCommissionRate,
            NewCommissionRate = request.CommissionRate,
            Reason = request.Reason
        };

        await _auditLogService.LogActionAsync(
            adminUserId,
            adminEmail,
            AuditActions.StoreCommissionChanged,
            EntityTypes.Store,
            storeId.ToString(),
            $"Store {store.StoreName} commission rate changed from {oldCommissionRate:P2} to {request.CommissionRate:P2} by admin. Reason: {request.Reason ?? "Not specified"}",
            JsonSerializer.Serialize(changes),
            ipAddress);

        return true;
    }
}
