# Mercato Partner & Public API Documentation

## Overview

This document describes the Partner and Public APIs for the Mercato multi-vendor e-commerce marketplace. These APIs enable external integrations with e-commerce platforms, ERP systems, WMS, and middleware solutions.

## Base URL

```
Development: https://localhost:5001/api
Production: https://api.mercato.com/api
```

## Authentication

### Public APIs
Public endpoints (under `/api/public`) require **no authentication** and can be accessed anonymously.

### User APIs
User endpoints require **JWT Bearer token** authentication. Obtain a token by logging in via `/api/auth/login`.

**Header:**
```
Authorization: Bearer <your-jwt-token>
```

### Partner APIs
Partner endpoints (under `/api/partner`) require **API Key** authentication. API keys are scoped to individual seller accounts.

**Header:**
```
X-API-Key: mrc_<your-api-key>
```

## API Token Management

Sellers can generate and manage API tokens for partner integrations via their dashboard.

### Create API Token
**POST** `/api/apitokens`

Creates a new API token for the authenticated seller.

**Authentication:** JWT Bearer token (Seller role required)

**Request Body:**
```json
{
  "name": "Baselinker Integration",
  "permissions": ["products:read", "products:write", "orders:read"],
  "expiresAt": "2025-12-31T23:59:59Z",
  "ipWhitelist": "192.168.1.100,10.0.0.50",
  "notes": "Integration for inventory sync"
}
```

**Response (201 Created):**
```json
{
  "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "token": "mrc_abc123def456ghi789jkl012mno345pqr678stu901vwx234yz",
  "name": "Baselinker Integration",
  "permissions": ["products:read", "products:write", "orders:read"],
  "createdAt": "2025-11-23T10:00:00Z",
  "expiresAt": "2025-12-31T23:59:59Z"
}
```

**Important:** The token value is only shown once at creation. Store it securely.

### List API Tokens
**GET** `/api/apitokens`

Lists all API tokens for the authenticated seller.

**Authentication:** JWT Bearer token

**Response (200 OK):**
```json
[
  {
    "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    "name": "Baselinker Integration",
    "permissions": ["products:read", "products:write", "orders:read"],
    "storeId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "createdAt": "2025-11-23T10:00:00Z",
    "expiresAt": "2025-12-31T23:59:59Z",
    "lastUsedAt": "2025-11-23T12:30:00Z",
    "isActive": true,
    "ipWhitelist": "192.168.1.100,10.0.0.50",
    "tokenPreview": "mrc_***...***"
  }
]
```

### Update API Token
**PUT** `/api/apitokens/{tokenId}`

Updates token name, active status, or notes.

**Authentication:** JWT Bearer token

**Request Body:**
```json
{
  "name": "Updated Integration Name",
  "isActive": false,
  "notes": "Disabled temporarily"
}
```

### Delete API Token
**DELETE** `/api/apitokens/{tokenId}`

Revokes an API token permanently.

**Authentication:** JWT Bearer token

**Response:** 204 No Content

## Public APIs

Public APIs provide read-only access to the product catalog. No authentication required.

### Get All Products
**GET** `/api/public/products`

Returns all published products from all stores.

**Response (200 OK):**
```json
[
  {
    "id": "product-guid",
    "storeId": "store-guid",
    "storeName": "Tech Store",
    "title": "Wireless Mouse",
    "description": "Ergonomic wireless mouse with USB receiver",
    "categoryName": "Electronics",
    "price": 29.99,
    "currency": "USD",
    "stockQuantity": 150,
    "imageUrls": ["https://example.com/image1.jpg"],
    "createdAt": "2025-11-20T10:00:00Z"
  }
]
```

### Get Product by ID
**GET** `/api/public/products/{productId}`

Returns a specific published product.

**Response (200 OK):** Same structure as above (single product)

### Get Store Products
**GET** `/api/public/stores/{storeId}/products`

Returns all published products for a specific store.

**Response (200 OK):** Array of products (same structure as Get All Products)

### Search Products
**POST** `/api/public/products/search`

Search and filter products with pagination.

**Request Body:**
```json
{
  "searchQuery": "wireless mouse",
  "categoryId": "category-guid",
  "minPrice": 10.00,
  "maxPrice": 100.00,
  "page": 1,
  "pageSize": 20
}
```

**Response (200 OK):**
```json
{
  "products": [ /* array of products */ ],
  "totalCount": 45,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

## Partner APIs

Partner APIs enable read and write operations for authenticated partners. Requires API Key authentication.

### Feature Flag

Write operations are controlled by the `PartnerApi:EnableWrite` feature flag in configuration. When disabled, write endpoints return 403 Forbidden.

**Configuration (appsettings.json):**
```json
{
  "PartnerApi": {
    "EnableWrite": false
  }
}
```

### Get Products
**GET** `/api/partner/products`

Returns all products for the partner's store.

**Authentication:** X-API-Key header  
**Permissions Required:** `products:read`

**Response (200 OK):**
```json
[
  {
    "id": "product-guid",
    "storeId": "store-guid",
    "sku": "WM-001",
    "title": "Wireless Mouse",
    "description": "Ergonomic wireless mouse",
    "categoryId": "category-guid",
    "categoryName": "Electronics",
    "price": 29.99,
    "currency": "USD",
    "stockQuantity": 150,
    "weight": 0.15,
    "imageUrls": ["https://example.com/image1.jpg"],
    "status": "Published",
    "createdAt": "2025-11-20T10:00:00Z",
    "updatedAt": "2025-11-22T15:30:00Z"
  }
]
```

### Get Product by ID
**GET** `/api/partner/products/{productId}`

Returns a specific product (must belong to the partner's store).

**Authentication:** X-API-Key header  
**Permissions Required:** `products:read`

**Response (200 OK):** Same as above (single product)

### Create Product
**POST** `/api/partner/products`

Creates a new product in the partner's store.

**Authentication:** X-API-Key header  
**Permissions Required:** `products:write`  
**Feature Flag:** `PartnerApi:EnableWrite` must be `true`

**Request Body:**
```json
{
  "sku": "WM-002",
  "title": "Gaming Mouse",
  "description": "High-precision gaming mouse with RGB lighting",
  "categoryId": "category-guid",
  "price": 59.99,
  "currency": "USD",
  "stockQuantity": 75,
  "weight": 0.18,
  "length": 12.5,
  "width": 7.0,
  "height": 4.5,
  "imageUrls": [
    "https://example.com/gaming-mouse-1.jpg",
    "https://example.com/gaming-mouse-2.jpg"
  ],
  "status": "Published"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Product created successfully",
  "product": { /* full product details */ }
}
```

### Update Product
**PUT** `/api/partner/products/{productId}`

Updates an existing product.

**Authentication:** X-API-Key header  
**Permissions Required:** `products:write`  
**Feature Flag:** `PartnerApi:EnableWrite` must be `true`

**Request Body:** Same as Create Product

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Product updated successfully",
  "product": { /* full product details */ }
}
```

### Update Product Stock
**PATCH** `/api/partner/products/{productId}/stock`

Updates only the stock quantity for a product.

**Authentication:** X-API-Key header  
**Permissions Required:** `products:write`  
**Feature Flag:** `PartnerApi:EnableWrite` must be `true`

**Request Body:**
```json
{
  "stockQuantity": 200
}
```

**Response (200 OK):**
```json
{
  "message": "Stock updated successfully",
  "stockQuantity": 200
}
```

### Get Orders
**GET** `/api/partner/orders`

Returns orders for the partner's seller account with optional filtering.

**Authentication:** X-API-Key header  
**Permissions Required:** `orders:read`

**Query Parameters:**
- `status` (optional): Filter by order status (Pending, Processing, Completed, Cancelled)
- `fromDate` (optional): Filter orders created after this date (ISO 8601)
- `toDate` (optional): Filter orders created before this date (ISO 8601)
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Results per page (default: 20, max: 100)

**Example:**
```
GET /api/partner/orders?status=Processing&page=1&pageSize=20
```

**Response (200 OK):**
```json
{
  "orders": [
    {
      "id": "order-guid",
      "orderNumber": "ORD-2025-001234",
      "userId": "user-guid",
      "status": "Processing",
      "totalAmount": 159.97,
      "createdAt": "2025-11-23T10:00:00Z",
      "items": [ /* order items */ ]
    }
  ],
  "totalCount": 45,
  "page": 1,
  "pageSize": 20
}
```

## Error Responses

All endpoints return consistent error responses:

### 400 Bad Request
```json
{
  "message": "Validation failed",
  "errors": {
    "Title": ["Title is required"],
    "Price": ["Price must be greater than 0"]
  }
}
```

### 401 Unauthorized
```json
{
  "message": "User not authenticated"
}
```

### 403 Forbidden
```json
{
  "message": "Partner API write operations are currently disabled"
}
```

or

```json
{
  "message": "API token is not scoped to a store"
}
```

### 404 Not Found
```json
{
  "message": "Product not found"
}
```

## Rate Limiting

**Note:** Rate limiting is not currently implemented but should be added before production deployment.

Recommended limits:
- Public APIs: 100 requests per minute per IP
- Partner APIs: 1000 requests per minute per API key
- User APIs: 60 requests per minute per user

## Security Best Practices

1. **Store API Keys Securely**: Never commit API keys to version control or expose them in client-side code.

2. **Use HTTPS**: Always use HTTPS in production to encrypt API traffic.

3. **IP Whitelisting**: Configure IP whitelisting for API tokens to restrict access to known IP addresses.

4. **Token Expiration**: Set expiration dates on API tokens and rotate them regularly.

5. **Minimal Permissions**: Grant only the minimum required permissions to each API token.

6. **Monitor Usage**: Regularly check the `lastUsedAt` field to identify unused or compromised tokens.

7. **Revoke Immediately**: If a token is compromised, revoke it immediately via DELETE `/api/apitokens/{tokenId}`.

## Webhook Integration (Future)

**Note:** Webhook support for order notifications is planned for a future release.

Planned webhook events:
- `order.created`
- `order.updated`
- `order.shipped`
- `order.completed`
- `product.low_stock`

## Support

For API support and integration assistance:
- Email: api-support@mercato.com
- Documentation: https://docs.mercato.com/api
- Status Page: https://status.mercato.com

## Changelog

### v1.0.0 (2025-11-23)
- Initial release
- Added API token authentication
- Added public read APIs for product catalog
- Added partner APIs for products (CRUD)
- Added partner APIs for orders (read)
- Added stock update endpoint
- Added feature flag for write operations
- Added Swagger/OpenAPI documentation
