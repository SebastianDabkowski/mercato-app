# Product Catalog - Data Model Extensibility Documentation

This document outlines how the Product Catalog data model is designed for extensibility to support future advanced features.

## Current Data Model Overview

### Product Entity
The Product entity is the core entity for the product catalog system. It includes:

- **Core Attributes**: Id, StoreId, SKU, Title, Description, Status
- **Pricing**: Price, Currency
- **Inventory**: StockQuantity
- **Categorization**: CategoryId (with navigation to Category)
- **Media**: ImageUrls (stored as JSON array)
- **Shipping**: Weight, Length, Width, Height (optional)
- **Timestamps**: CreatedAt, UpdatedAt

### Category Entity
The Category entity enables hierarchical product organization:

- **Core Attributes**: Id, Name, Description
- **Hierarchy**: ParentCategoryId (self-referencing for subcategories)
- **Status**: IsActive
- **Timestamps**: CreatedAt

## Extensibility Strategies

### 1. Product Attributes and Filters

**Current State**: Basic product fields are directly in the Product entity.

**Future Extension Options**:

#### Option A: JSON Column for Custom Attributes
Add a flexible JSON column to store custom attributes without schema changes:

```csharp
public class Product
{
    // ... existing properties
    
    /// <summary>
    /// Custom attributes stored as JSON for extensibility.
    /// Example: {"color": "Red", "size": "Large", "material": "Cotton"}
    /// </summary>
    [MaxLength(4000)]
    public string? CustomAttributes { get; set; }
}
```

**Pros**: 
- No schema changes needed for new attributes
- Flexible for different product types
- Easy to add product-specific fields

**Cons**:
- Harder to query/filter
- No strong typing
- Performance overhead for complex queries

#### Option B: Entity-Attribute-Value (EAV) Pattern
Create separate tables for attributes:

```csharp
public class ProductAttribute
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string AttributeName { get; set; }
    public string AttributeValue { get; set; }
    public string AttributeType { get; set; } // string, number, boolean, etc.
    
    public Product Product { get; set; }
}

public class CategoryAttribute
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string AttributeName { get; set; }
    public string DisplayName { get; set; }
    public string AttributeType { get; set; }
    public bool IsRequired { get; set; }
    
    public Category Category { get; set; }
}
```

**Pros**:
- Strongly typed attributes
- Better queryability
- Can enforce attribute requirements per category
- Supports attribute templates by category

**Cons**:
- More complex schema
- Joins required for queries
- More storage overhead

**Recommended Approach**: Use Option B (EAV) for formal attributes and filters, and Option A (JSON) for ad-hoc properties.

### 2. Product Variants

For products with multiple variants (e.g., sizes, colors):

```csharp
public class ProductVariant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string SKU { get; set; }
    public string VariantName { get; set; } // e.g., "Large Red"
    public decimal PriceAdjustment { get; set; } // +/- from base price
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    
    // Variant attributes (stored as JSON)
    public string VariantAttributes { get; set; } // {"size": "Large", "color": "Red"}
    
    public Product Product { get; set; }
}
```

**Implementation Notes**:
- Base Product holds common information
- ProductVariant holds variant-specific data
- Each variant has its own SKU, stock, and optional price adjustment
- Variant attributes stored as JSON for flexibility

### 3. Product Tags and Taxonomies

For better searchability and multi-dimensional categorization:

```csharp
public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductTag
{
    public Guid ProductId { get; set; }
    public Guid TagId { get; set; }
    
    public Product Product { get; set; }
    public Tag Tag { get; set; }
}
```

**Benefits**:
- Products can belong to multiple tags
- Tags enable cross-category filtering
- Supports "Related Products" features
- Better search and discovery

### 4. Promotions and Pricing Rules

For advanced pricing and promotional features:

```csharp
public class ProductPromotion
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string PromotionType { get; set; } // Discount, BuyOneGetOne, Bundle, etc.
    public decimal DiscountAmount { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    
    // Conditions stored as JSON for flexibility
    public string? Conditions { get; set; } // {"minQuantity": 2, "maxUsesPerCustomer": 1}
    
    public Product Product { get; set; }
}

public class PriceHistory
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public Product Product { get; set; }
}
```

**Use Cases**:
- Time-limited discounts
- Quantity-based pricing
- Bundle deals
- Price tracking and analytics

### 5. Product Reviews and Ratings

For buyer feedback and social proof:

```csharp
public class ProductReview
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string UserId { get; set; } // From Users module
    public int Rating { get; set; } // 1-5 stars
    public string? Title { get; set; }
    public string? ReviewText { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public Product Product { get; set; }
}
```

**Aggregated Metrics** (can be calculated or cached):
```csharp
public class Product
{
    // ... existing properties
    
    public decimal? AverageRating { get; set; }
    public int ReviewCount { get; set; }
}
```

### 6. Product Media and Assets

For rich product presentation:

```csharp
public class ProductMedia
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string MediaType { get; set; } // Image, Video, Document
    public string Url { get; set; }
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Product Product { get; set; }
}
```

**Benefits**:
- Multiple images per product
- Support for videos
- Product documentation (PDFs, manuals)
- Proper ordering and primary image selection

### 7. Inventory Tracking and Notifications

For better stock management:

```csharp
public class InventoryLog
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int QuantityChange { get; set; }
    public int NewQuantity { get; set; }
    public string Reason { get; set; } // Sale, Restock, Adjustment, Return
    public string? Reference { get; set; } // Order ID or other reference
    public DateTime CreatedAt { get; set; }
    
    public Product Product { get; set; }
}

public class LowStockAlert
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int ThresholdQuantity { get; set; }
    public bool IsActive { get; set; }
    
    public Product Product { get; set; }
}
```

## Database Schema Design Principles

1. **Modular Schema**: Keep product catalog tables in the `productcatalog` schema
2. **Indexing Strategy**: 
   - Index frequently filtered fields (CategoryId, Status, Price, CreatedAt)
   - Consider full-text search indexes for Title/Description
   - Composite indexes for common query patterns

3. **Soft Deletes**: Use status flags instead of hard deletes for audit trail
4. **Timestamps**: Always include CreatedAt, consider UpdatedAt for trackability
5. **Foreign Keys**: Use proper foreign key constraints but consider OnDelete behavior for cross-module references

## Migration Strategy

When adding new features:

1. **Backward Compatible Changes**: Add new optional columns/tables
2. **Data Migration**: Use EF Core migrations with data seeding when needed
3. **Feature Flags**: Consider feature flags for gradual rollout of new functionality
4. **Testing**: Always test migrations on a copy of production data

## Performance Considerations

1. **Lazy Loading**: Disable lazy loading by default, use explicit Include()
2. **Pagination**: Always paginate large result sets
3. **Caching**: Consider caching category trees and frequently accessed products
4. **Read Models**: For complex queries, consider denormalized read models (CQRS pattern)

## Integration Points

### Cross-Module Communication

The Product Catalog module has integration points with:

1. **SellerPanel Module**: StoreId foreign key (products belong to stores)
2. **Cart Module**: ProductId references for cart items
3. **History Module**: ProductId references in order items
4. **Users Module**: User reviews, wishlist features
5. **Payments Module**: Product pricing for transaction calculations

**Integration Patterns**:
- Use service-to-service calls for cross-module data
- Avoid direct database joins across module boundaries
- Consider eventual consistency for non-critical data
- Use events/messages for decoupled communication

## Recommended Next Steps

1. **Phase 1** (Current): Basic product and category management ✓
2. **Phase 2**: Add product tags and improved search
3. **Phase 3**: Implement product attributes and filters (EAV pattern)
4. **Phase 4**: Add product variants for size/color options
5. **Phase 5**: Implement promotions and dynamic pricing
6. **Phase 6**: Add reviews and ratings system
7. **Phase 7**: Advanced inventory tracking and alerts

## Summary

The current Product Catalog data model provides a solid foundation that can be extended in multiple dimensions:
- **Vertical Extension**: Add new entity types (variants, reviews, promotions)
- **Horizontal Extension**: Add attributes via JSON or EAV pattern
- **Cross-cutting Extensions**: Tags, media, search improvements

The design prioritizes flexibility while maintaining good performance and data integrity. All proposed extensions maintain backward compatibility with the current schema.
