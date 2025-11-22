using Microsoft.EntityFrameworkCore;
using SD.Mercato.Administration.DTOs;
using SD.Mercato.Administration.Models;
using SD.Mercato.ProductCatalog.Data;
using SD.Mercato.ProductCatalog.Models;
using System.Text.Json;

namespace SD.Mercato.Administration.Services;

/// <summary>
/// Service for admin category management operations.
/// </summary>
public class AdminCategoryService : IAdminCategoryService
{
    private readonly ProductCatalogDbContext _catalogContext;
    private readonly IAuditLogService _auditLogService;

    public AdminCategoryService(
        ProductCatalogDbContext catalogContext,
        IAuditLogService auditLogService)
    {
        _catalogContext = catalogContext;
        _auditLogService = auditLogService;
    }

    public async Task<List<AdminCategoryDto>> GetAllCategoriesAsync()
    {
        // Fetch all categories
        var categories = await _catalogContext.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();

        // Fetch product counts for all categories in one query
        var productCounts = await _catalogContext.Products
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToListAsync();
        var productCountDict = productCounts.ToDictionary(x => x.CategoryId, x => x.Count);

        // Get all parent category IDs referenced
        var parentCategoryIds = categories
            .Where(c => c.ParentCategoryId.HasValue)
            .Select(c => c.ParentCategoryId!.Value)
            .Distinct()
            .ToList();

        // Fetch all parent categories in one query
        var parentCategories = await _catalogContext.Categories
            .Where(c => parentCategoryIds.Contains(c.Id))
            .ToListAsync();
        var parentCategoryDict = parentCategories.ToDictionary(c => c.Id, c => c.Name);

        // Build DTOs
        var categoryDtos = categories.Select(category => new AdminCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategoryId.HasValue
                ? parentCategoryDict.GetValueOrDefault(category.ParentCategoryId.Value)
                : null,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            DefaultCommissionRate = category.DefaultCommissionRate,
            ProductCount = productCountDict.GetValueOrDefault(category.Id, 0)
        }).ToList();

        return categoryDtos;
    }

    public async Task<AdminCategoryDto?> GetCategoryByIdAsync(Guid categoryId)
    {
        var category = await _catalogContext.Categories.FindAsync(categoryId);
        if (category == null)
        {
            return null;
        }

        var productCount = await _catalogContext.Products
            .CountAsync(p => p.CategoryId == categoryId);

        var parentCategory = category.ParentCategoryId.HasValue
            ? await _catalogContext.Categories.FindAsync(category.ParentCategoryId.Value)
            : null;

        return new AdminCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = parentCategory?.Name,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            DefaultCommissionRate = category.DefaultCommissionRate,
            ProductCount = productCount
        };
    }

    public async Task<AdminCategoryDto> CreateCategoryAsync(
        AdminCreateCategoryRequest request,
        string adminUserId,
        string adminEmail,
        string? ipAddress)
    {
        // Validate parent category exists if specified
        if (request.ParentCategoryId.HasValue)
        {
            var parentExists = await _catalogContext.Categories
                .AnyAsync(c => c.Id == request.ParentCategoryId.Value);
            
            if (!parentExists)
            {
                throw new InvalidOperationException("Parent category not found");
            }
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            DefaultCommissionRate = request.DefaultCommissionRate
        };

        _catalogContext.Categories.Add(category);
        await _catalogContext.SaveChangesAsync();

        // Log the action
        var changes = new
        {
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId,
            DefaultCommissionRate = request.DefaultCommissionRate
        };

        await _auditLogService.LogActionAsync(
            adminUserId,
            adminEmail,
            AuditActions.CategoryCreated,
            EntityTypes.Category,
            category.Id.ToString(),
            $"Category '{category.Name}' created by admin",
            JsonSerializer.Serialize(changes),
            ipAddress);

        return await GetCategoryByIdAsync(category.Id) ?? throw new InvalidOperationException("Category not found after creation");
    }

    public async Task<AdminCategoryDto?> UpdateCategoryAsync(
        Guid categoryId,
        AdminUpdateCategoryRequest request,
        string adminUserId,
        string adminEmail,
        string? ipAddress)
    {
        var category = await _catalogContext.Categories.FindAsync(categoryId);
        if (category == null)
        {
            return null;
        }

        // Validate parent category exists if specified
        if (request.ParentCategoryId.HasValue)
        {
            // Prevent circular reference
            if (request.ParentCategoryId.Value == categoryId)
            {
                throw new InvalidOperationException("Category cannot be its own parent");
            }

            var parentExists = await _catalogContext.Categories
                .AnyAsync(c => c.Id == request.ParentCategoryId.Value);
            
            if (!parentExists)
            {
                throw new InvalidOperationException("Parent category not found");
            }

            // Check for circular hierarchy chains
            if (await WouldCreateCircularReferenceAsync(categoryId, request.ParentCategoryId.Value))
            {
                throw new InvalidOperationException("Setting this parent would create a circular reference in the category hierarchy");
            }
        }

        var changes = new Dictionary<string, object>();

        if (category.Name != request.Name)
        {
            changes["Name"] = new { Old = category.Name, New = request.Name };
            category.Name = request.Name;
        }

        if (category.Description != request.Description)
        {
            changes["Description"] = new { Old = category.Description, New = request.Description };
            category.Description = request.Description;
        }

        if (category.ParentCategoryId != request.ParentCategoryId)
        {
            changes["ParentCategoryId"] = new { Old = category.ParentCategoryId, New = request.ParentCategoryId };
            category.ParentCategoryId = request.ParentCategoryId;
        }

        if (category.DefaultCommissionRate != request.DefaultCommissionRate)
        {
            changes["DefaultCommissionRate"] = new { Old = category.DefaultCommissionRate, New = request.DefaultCommissionRate };
            category.DefaultCommissionRate = request.DefaultCommissionRate;
        }

        if (request.IsActive.HasValue && category.IsActive != request.IsActive.Value)
        {
            changes["IsActive"] = new { Old = category.IsActive, New = request.IsActive.Value };
            category.IsActive = request.IsActive.Value;
        }

        if (changes.Count == 0)
        {
            return await GetCategoryByIdAsync(categoryId);
        }

        await _catalogContext.SaveChangesAsync();

        // Log the action
        var action = request.IsActive.HasValue && !request.IsActive.Value
            ? AuditActions.CategoryDeactivated
            : request.IsActive.HasValue && request.IsActive.Value
            ? AuditActions.CategoryActivated
            : changes.ContainsKey("DefaultCommissionRate")
            ? AuditActions.CategoryCommissionChanged
            : AuditActions.CategoryUpdated;

        await _auditLogService.LogActionAsync(
            adminUserId,
            adminEmail,
            action,
            EntityTypes.Category,
            categoryId.ToString(),
            $"Category '{category.Name}' updated by admin. Reason: {request.Reason ?? "Not specified"}",
            JsonSerializer.Serialize(new { Changes = changes, Reason = request.Reason }),
            ipAddress);

        return await GetCategoryByIdAsync(categoryId);
    }

    public async Task<bool> DeleteCategoryAsync(
        Guid categoryId,
        string adminUserId,
        string adminEmail,
        string? ipAddress)
    {
        var category = await _catalogContext.Categories.FindAsync(categoryId);
        if (category == null)
        {
            return false;
        }

        // Check if category has products
        var hasProducts = await _catalogContext.Products.AnyAsync(p => p.CategoryId == categoryId);
        if (hasProducts)
        {
            throw new InvalidOperationException("Cannot delete category with existing products. Deactivate it instead.");
        }

        // Check if category has child categories
        var hasChildren = await _catalogContext.Categories.AnyAsync(c => c.ParentCategoryId == categoryId);
        if (hasChildren)
        {
            throw new InvalidOperationException("Cannot delete category with child categories.");
        }

        // Soft delete by deactivating
        category.IsActive = false;
        await _catalogContext.SaveChangesAsync();

        // Log the action
        await _auditLogService.LogActionAsync(
            adminUserId,
            adminEmail,
            AuditActions.CategoryDeactivated,
            EntityTypes.Category,
            categoryId.ToString(),
            $"Category '{category.Name}' deleted (deactivated) by admin",
            null,
            ipAddress);

        return true;
    }

    /// <summary>
    /// Checks if setting a parent would create a circular reference in the category hierarchy.
    /// This checks both upward (ancestors) and downward (descendants) to ensure no cycles.
    /// </summary>
    private async Task<bool> WouldCreateCircularReferenceAsync(Guid categoryId, Guid proposedParentId)
    {
        // First check: traverse up from proposed parent to see if we reach the category
        // This catches: A -> B, trying to set B -> A
        var currentId = proposedParentId;
        var visitedIds = new HashSet<Guid>();

        while (currentId != Guid.Empty)
        {
            if (currentId == categoryId)
            {
                // The proposed parent is a descendant of this category
                return true;
            }

            if (visitedIds.Contains(currentId))
            {
                // Found a cycle in existing hierarchy (shouldn't happen, but protect against it)
                return true;
            }

            visitedIds.Add(currentId);

            var parent = await _catalogContext.Categories.FindAsync(currentId);
            if (parent == null || !parent.ParentCategoryId.HasValue)
            {
                // Reached the root or invalid parent
                break;
            }

            currentId = parent.ParentCategoryId.Value;
        }

        // Second check: ensure the proposed parent is not a descendant of this category
        // This is already covered by the check above, but we'll keep it for clarity
        return await IsDescendantOfAsync(proposedParentId, categoryId);
    }

    /// <summary>
    /// Checks if potentialDescendant is a descendant of ancestorId.
    /// </summary>
    private async Task<bool> IsDescendantOfAsync(Guid potentialDescendantId, Guid ancestorId)
    {
        var currentId = potentialDescendantId;
        var visitedIds = new HashSet<Guid>();

        while (currentId != Guid.Empty)
        {
            if (currentId == ancestorId)
            {
                return true;
            }

            if (visitedIds.Contains(currentId))
            {
                // Cycle detected (shouldn't happen in valid data)
                break;
            }

            visitedIds.Add(currentId);

            var category = await _catalogContext.Categories.FindAsync(currentId);
            if (category == null || !category.ParentCategoryId.HasValue)
            {
                break;
            }

            currentId = category.ParentCategoryId.Value;
        }

        return false;
    }
}
