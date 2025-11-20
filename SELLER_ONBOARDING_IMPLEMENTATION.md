# Seller Onboarding and Store Profile Management - Implementation Guide

## Overview

This document describes the implementation of seller onboarding and store profile management features for the Mercato marketplace platform.

## Implemented Features

### 1. Seller Onboarding
- Sellers can create a store account as either a **Company** or **Individual/Hobby seller**
- Store creation requires:
  - Unique store name (URL-friendly, lowercase with hyphens)
  - Display name
  - Contact information (email, phone)
  - Store type selection
  - Business details (for company stores: business name, tax ID)
  - Optional: logo, description, address, bank account details, delivery/return info

### 2. Store Profile Management
- Sellers can update their store profile
- Update includes: display name, description, logo, contact info, business details, delivery/return policies
- Store name cannot be changed after creation (ensures URL stability)

### 3. Public Store View
- Buyers can view store profiles at `/api/stores/public/{storeName}`
- Public profile includes: store name, display name, description, logo, delivery info, return info
- Products listing integration ready (via ProductCatalog module)

### 4. Product Management
- Sellers can create, update, and delete products
- Products require: SKU, title, description, category, price, stock quantity
- Optional: images, weight, dimensions (for shipping)
- Product status: Draft, Published, Archived
- Published products require at least one image and appear in public catalog

### 5. Category Management
- Product categories with hierarchical support
- Pre-seeded categories: Electronics, Clothing, Home & Garden, Books, Sports & Outdoors, Toys & Games, Health & Beauty, Food & Beverages
- Categories can be created and listed

## Architecture

### Modules Implemented

#### 1. SellerPanel Module (`SD.Mercato.SellerPanel`)
- **Models**: `Store` entity with all required fields
- **Data**: `SellerPanelDbContext` with separate schema `sellerpanel`
- **DTOs**: `CreateStoreRequest`, `UpdateStoreProfileRequest`, `StoreDto`, `PublicStoreProfileDto`
- **Services**: `IStoreService`, `StoreService` for store management
- **Features**:
  - Store creation with validation
  - Store profile updates
  - Store name availability checking
  - Public store profile retrieval
  - One store per user enforcement

#### 2. ProductCatalog Module (`SD.Mercato.ProductCatalog`)
- **Models**: `Product` and `Category` entities
- **Data**: `ProductCatalogDbContext` with separate schema `productcatalog`
- **DTOs**: `CreateProductRequest`, `UpdateProductRequest`, `ProductDto`, `PublicProductDto`, `CategoryDto`
- **Services**: 
  - `IProductService`, `ProductService` for product management
  - `ICategoryService`, `CategoryService` for category management
- **Features**:
  - Product CRUD operations with validation
  - SKU uniqueness within store
  - Category-based organization
  - Stock management
  - Image URL storage (JSON array)
  - Product status workflow (Draft → Published → Archived)

### API Endpoints

#### Store Management (`/api/stores`)

```
POST   /api/stores                      - Create new store (authenticated)
PUT    /api/stores/{storeId}            - Update store profile (authenticated, owner only)
GET    /api/stores/my-store             - Get authenticated user's store
GET    /api/stores/{storeId}            - Get store by ID
GET    /api/stores/check-name/{name}    - Check store name availability
GET    /api/stores/public/{storeName}   - Get public store profile (buyer-facing)
```

#### Product Management (`/api/products`)

```
POST   /api/products                    - Create new product (authenticated seller)
PUT    /api/products/{productId}        - Update product (authenticated seller, owner only)
GET    /api/products/{productId}        - Get product by ID
GET    /api/products/my-products        - Get authenticated seller's products
GET    /api/products/store/{storeId}    - Get published products for store (public)
DELETE /api/products/{productId}        - Delete product (authenticated seller, owner only)
```

#### Category Management (`/api/categories`)

```
POST   /api/categories                  - Create new category (authenticated)
GET    /api/categories                  - Get all active categories
GET    /api/categories/{categoryId}     - Get category by ID
```

## Database Schema

### SellerPanel Schema

**Table: `sellerpanel.Stores`**
- `Id` (Guid, PK)
- `OwnerUserId` (string, FK to Users)
- `StoreName` (string, unique index) - URL-friendly identifier
- `DisplayName` (string) - Human-readable name
- `Description` (string, nullable)
- `LogoUrl` (string, nullable)
- `ContactEmail` (string)
- `PhoneNumber` (string, nullable)
- `StoreType` (string) - "Company" or "Individual"
- `BusinessName` (string, nullable)
- `TaxId` (string, nullable)
- Address fields (Line1, Line2, City, State, PostalCode, Country)
- `BankAccountDetails` (string, nullable) - TODO: Should be encrypted
- `CommissionRate` (decimal) - Default 0.15 (15%)
- `IsActive` (bool)
- `IsVerified` (bool)
- `CreatedAt` (DateTime)
- `UpdatedAt` (DateTime, nullable)
- `DeliveryInfo` (string, nullable)
- `ReturnInfo` (string, nullable)

### ProductCatalog Schema

**Table: `productcatalog.Categories`**
- `Id` (Guid, PK)
- `Name` (string, unique index)
- `Description` (string, nullable)
- `ParentCategoryId` (Guid, nullable, self-referencing FK)
- `IsActive` (bool)
- `CreatedAt` (DateTime)

**Table: `productcatalog.Products`**
- `Id` (Guid, PK)
- `StoreId` (Guid, FK to Stores) - Composite unique index with SKU
- `SKU` (string) - Unique within store
- `Title` (string)
- `Description` (string)
- `CategoryId` (Guid, FK to Categories)
- `Price` (decimal)
- `StockQuantity` (int)
- `Weight`, `Length`, `Width`, `Height` (decimal, nullable)
- `ImageUrls` (string) - JSON array of URLs
- `Status` (string) - "Draft", "Published", or "Archived"
- `CreatedAt` (DateTime)
- `UpdatedAt` (DateTime, nullable)

## Business Rules Implemented

### Store Creation
- ✅ Store name must be unique across platform
- ✅ Store name must be URL-friendly (lowercase, alphanumeric, hyphens only)
- ✅ One store per user (enforced)
- ✅ Company stores require business name and tax ID
- ✅ Default commission rate: 15%
- ✅ New stores are active but unverified

### Product Management
- ✅ SKU must be unique within seller's store
- ✅ Price must be > 0
- ✅ Stock quantity cannot be negative
- ✅ Published products must have at least one image
- ✅ Category must exist and be active
- ✅ Products can only be managed by store owner

### Store Profile
- ✅ Only store owner can update profile
- ✅ Store name is immutable after creation
- ✅ Public profiles only show active stores

## Validation & Security

### Input Validation
- All DTOs use Data Annotations for validation
- Email format validation
- Phone number format validation
- Range validation for numeric fields
- String length constraints

### Authorization
- Store creation: authenticated users
- Store updates: authenticated store owner only
- Product management: authenticated store owner only
- Public endpoints: no authentication required
- Category creation: authenticated users (TODO: restrict to admin role)

### Data Integrity
- Unique constraints on store names
- Composite unique constraint on StoreId + SKU
- Foreign key relationships with proper delete behavior
- Indexes on frequently queried fields

## Migration Strategy

Two separate migrations created:
1. `20251120115301_InitialSellerPanelCreate` - Creates sellerpanel schema and Stores table
2. `20251120115729_InitialProductCatalogCreate` - Creates productcatalog schema, Categories and Products tables

To apply migrations:
```bash
cd src/API/SD.Mercato.API
dotnet ef database update --context UsersDbContext
dotnet ef database update --context SellerPanelDbContext
dotnet ef database update --context ProductCatalogDbContext
```

## Data Seeding

### Roles (Users Module)
- Buyer, Seller, Administrator roles are auto-seeded

### Categories (ProductCatalog Module)
- 8 default categories are auto-seeded on first run:
  - Electronics
  - Clothing
  - Home & Garden
  - Books
  - Sports & Outdoors
  - Toys & Games
  - Health & Beauty
  - Food & Beverages

## Testing the Implementation

### 1. Seller Onboarding Flow

**Register as a seller:**
```bash
POST /api/auth/register
{
  "email": "seller@example.com",
  "password": "Password123",
  "firstName": "John",
  "lastName": "Doe",
  "role": "Seller"
}
```

**Create a store:**
```bash
POST /api/stores
Authorization: Bearer {token}
{
  "storeName": "johns-electronics",
  "displayName": "John's Electronics",
  "description": "Quality electronics at great prices",
  "contactEmail": "seller@example.com",
  "phoneNumber": "+1234567890",
  "storeType": "Individual"
}
```

### 2. Product Management Flow

**Get categories:**
```bash
GET /api/categories
```

**Create a product:**
```bash
POST /api/products
Authorization: Bearer {token}
{
  "sku": "LAPTOP-001",
  "title": "High Performance Laptop",
  "description": "15-inch laptop with 16GB RAM",
  "categoryId": "{categoryId}",
  "price": 999.99,
  "stockQuantity": 10,
  "imageUrls": ["https://example.com/laptop.jpg"],
  "status": "Published"
}
```

**Get my products:**
```bash
GET /api/products/my-products
Authorization: Bearer {token}
```

### 3. Public Store View

**View store profile:**
```bash
GET /api/stores/public/johns-electronics
```

**View store products:**
```bash
GET /api/products/store/{storeId}
```

## TODO Items & Future Enhancements

### Security
- [ ] Encrypt bank account details in database
- [ ] Implement secure file upload for logos and product images
- [ ] Add rate limiting for public endpoints

### Business Logic
- [ ] Implement seller verification workflow
- [ ] Add admin approval for new stores (optional)
- [ ] Implement minimum price requirements
- [ ] Add restricted categories requiring approval
- [ ] Stock reservation during checkout
- [ ] Handle price changes for items in cart

### Features
- [ ] Product search and filtering
- [ ] Store ratings and reviews
- [ ] Product variants (size, color)
- [ ] Bulk product upload
- [ ] Store analytics dashboard
- [ ] Cross-module integration for store name in PublicProductDto

### Performance
- [ ] Add caching for public store profiles
- [ ] Add caching for categories
- [ ] Optimize product queries with pagination
- [ ] Add full-text search for products

## Dependencies

### NuGet Packages Added
- Microsoft.EntityFrameworkCore (9.0.0)
- Microsoft.EntityFrameworkCore.SqlServer (9.0.0)
- Microsoft.EntityFrameworkCore.Design (9.0.0)

### Module Dependencies
- SellerPanel: depends on Users module (OwnerUserId reference)
- ProductCatalog: depends on SellerPanel (StoreId reference)
- API: depends on Users, SellerPanel, ProductCatalog

## Conclusion

This implementation provides a complete foundation for seller onboarding and store management in the Mercato marketplace. The modular architecture allows for easy extension and maintenance, while the comprehensive validation and security measures ensure data integrity and proper authorization.

All core requirements from the issue have been addressed:
✅ Seller account creation (company or individual)
✅ Store profile creation with required information
✅ Store profile editing
✅ Public store view at mercato.pl/{store-name}
✅ Product management infrastructure
✅ Category system
✅ Support for later verification workflows
