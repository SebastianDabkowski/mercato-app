# Mercato API Usage Examples

This file contains practical examples of using the Mercato Partner and Public APIs.

## Table of Contents
1. [Getting Started](#getting-started)
2. [Managing API Tokens](#managing-api-tokens)
3. [Public API Examples](#public-api-examples)
4. [Partner API Examples](#partner-api-examples)
5. [Error Handling](#error-handling)

## Getting Started

### Prerequisites
- For User/Partner APIs: You need a Mercato seller account
- For Partner APIs: Generate an API token from your dashboard
- Tools: curl, Postman, or any HTTP client

### Base URLs
```
Development: https://localhost:5001/api
Production: https://api.mercato.com/api
```

## Managing API Tokens

### 1. Login to Get JWT Token

First, authenticate as a seller to manage API tokens:

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "seller@example.com",
    "password": "YourPassword123"
  }'
```

**Response:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "user-guid",
    "email": "seller@example.com",
    "role": "Seller"
  }
}
```

Save the `token` value for subsequent requests.

### 2. Create an API Token

```bash
curl -X POST https://localhost:5001/api/apitokens \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Production Integration",
    "permissions": ["products:read", "products:write", "orders:read"],
    "expiresAt": "2026-12-31T23:59:59Z",
    "notes": "Main production API key"
  }'
```

**Response:**
```json
{
  "id": "token-guid",
  "token": "mrc_abc123def456...",
  "name": "Production Integration",
  "permissions": ["products:read", "products:write", "orders:read"],
  "createdAt": "2025-11-23T10:00:00Z",
  "expiresAt": "2026-12-31T23:59:59Z"
}
```

**Important:** Save the `token` value securely. It won't be shown again.

### 3. List Your API Tokens

```bash
curl -X GET https://localhost:5001/api/apitokens \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### 4. Revoke an API Token

```bash
curl -X DELETE https://localhost:5001/api/apitokens/TOKEN_GUID \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Public API Examples

These endpoints require no authentication.

### Get All Products

```bash
curl -X GET https://localhost:5001/api/public/products
```

### Get Product by ID

```bash
curl -X GET https://localhost:5001/api/public/products/PRODUCT_GUID
```

### Search Products

```bash
curl -X POST https://localhost:5001/api/public/products/search \
  -H "Content-Type: application/json" \
  -d '{
    "searchQuery": "wireless",
    "minPrice": 10,
    "maxPrice": 100,
    "page": 1,
    "pageSize": 20
  }'
```

### Search by Category

```bash
curl -X POST https://localhost:5001/api/public/products/search \
  -H "Content-Type: application/json" \
  -d '{
    "categoryId": "CATEGORY_GUID",
    "page": 1,
    "pageSize": 50
  }'
```

## Partner API Examples

These endpoints require API Key authentication via `X-API-Key` header.

### Get All Your Products

```bash
curl -X GET https://localhost:5001/api/partner/products \
  -H "X-API-Key: mrc_your_api_key_here"
```

### Get Specific Product

```bash
curl -X GET https://localhost:5001/api/partner/products/PRODUCT_GUID \
  -H "X-API-Key: mrc_your_api_key_here"
```

### Create a New Product

```bash
curl -X POST https://localhost:5001/api/partner/products \
  -H "X-API-Key: mrc_your_api_key_here" \
  -H "Content-Type: application/json" \
  -d '{
    "sku": "LAPTOP-001",
    "title": "Professional Laptop 15 inch",
    "description": "High-performance laptop with 16GB RAM and 512GB SSD",
    "categoryId": "electronics-category-guid",
    "price": 899.99,
    "currency": "USD",
    "stockQuantity": 25,
    "weight": 2.5,
    "length": 35.0,
    "width": 24.0,
    "height": 2.5,
    "imageUrls": [
      "https://example.com/laptop-front.jpg",
      "https://example.com/laptop-side.jpg"
    ],
    "status": "Published"
  }'
```

### Update a Product

```bash
curl -X PUT https://localhost:5001/api/partner/products/PRODUCT_GUID \
  -H "X-API-Key: mrc_your_api_key_here" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Professional Laptop 15 inch - Updated",
    "description": "High-performance laptop with 16GB RAM and 1TB SSD",
    "categoryId": "electronics-category-guid",
    "price": 949.99,
    "currency": "USD",
    "stockQuantity": 20,
    "weight": 2.5,
    "imageUrls": [
      "https://example.com/laptop-front.jpg",
      "https://example.com/laptop-side.jpg"
    ],
    "status": "Published"
  }'
```

### Update Stock Only

```bash
curl -X PATCH https://localhost:5001/api/partner/products/PRODUCT_GUID/stock \
  -H "X-API-Key: mrc_your_api_key_here" \
  -H "Content-Type: application/json" \
  -d '{
    "stockQuantity": 50
  }'
```

### Get Your Orders

```bash
curl -X GET https://localhost:5001/api/partner/orders \
  -H "X-API-Key: mrc_your_api_key_here"
```

### Get Orders with Filters

```bash
# Get processing orders from last 30 days
curl -X GET "https://localhost:5001/api/partner/orders?status=Processing&fromDate=2025-10-24T00:00:00Z&page=1&pageSize=20" \
  -H "X-API-Key: mrc_your_api_key_here"
```

## Error Handling

### Example: Handle Validation Errors

```bash
# This will fail validation (missing required fields)
curl -X POST https://localhost:5001/api/partner/products \
  -H "X-API-Key: mrc_your_api_key_here" \
  -H "Content-Type: application/json" \
  -d '{
    "sku": "TEST-001"
  }'
```

**Error Response (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Title": ["Title is required"],
    "Description": ["Description is required"],
    "CategoryId": ["Category ID is required"],
    "Price": ["Price is required"]
  }
}
```

### Example: Handle Unauthorized Access

```bash
# Missing or invalid API key
curl -X GET https://localhost:5001/api/partner/products \
  -H "X-API-Key: invalid_key"
```

**Error Response (401 Unauthorized):**
```json
{
  "message": "Invalid API key"
}
```

### Example: Handle Feature Flag Disabled

```bash
# Try to create product when write is disabled
curl -X POST https://localhost:5001/api/partner/products \
  -H "X-API-Key: mrc_your_api_key_here" \
  -H "Content-Type: application/json" \
  -d '{ ... }'
```

**Error Response (403 Forbidden):**
```json
{
  "message": "Partner API write operations are currently disabled"
}
```

## Integration Examples

### Node.js / JavaScript

```javascript
// Using fetch API
const apiKey = 'mrc_your_api_key_here';
const baseUrl = 'https://localhost:5001/api';

// Get products
async function getProducts() {
  const response = await fetch(`${baseUrl}/partner/products`, {
    headers: {
      'X-API-Key': apiKey
    }
  });
  
  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }
  
  const products = await response.json();
  return products;
}

// Create product
async function createProduct(productData) {
  const response = await fetch(`${baseUrl}/partner/products`, {
    method: 'POST',
    headers: {
      'X-API-Key': apiKey,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(productData)
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(JSON.stringify(error));
  }
  
  return await response.json();
}

// Update stock
async function updateStock(productId, quantity) {
  const response = await fetch(`${baseUrl}/partner/products/${productId}/stock`, {
    method: 'PATCH',
    headers: {
      'X-API-Key': apiKey,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ stockQuantity: quantity })
  });
  
  return await response.json();
}
```

### Python

```python
import requests

api_key = 'mrc_your_api_key_here'
base_url = 'https://localhost:5001/api'

headers = {
    'X-API-Key': api_key,
    'Content-Type': 'application/json'
}

# Get products
def get_products():
    response = requests.get(f'{base_url}/partner/products', headers=headers)
    response.raise_for_status()
    return response.json()

# Create product
def create_product(product_data):
    response = requests.post(
        f'{base_url}/partner/products',
        headers=headers,
        json=product_data
    )
    response.raise_for_status()
    return response.json()

# Update stock
def update_stock(product_id, quantity):
    response = requests.patch(
        f'{base_url}/partner/products/{product_id}/stock',
        headers=headers,
        json={'stockQuantity': quantity}
    )
    response.raise_for_status()
    return response.json()

# Example usage
if __name__ == '__main__':
    # Get all products
    products = get_products()
    print(f'Found {len(products)} products')
    
    # Update stock for first product
    if products:
        product_id = products[0]['id']
        result = update_stock(product_id, 100)
        print(f'Stock updated: {result}')
```

### PHP

```php
<?php

$apiKey = 'mrc_your_api_key_here';
$baseUrl = 'https://localhost:5001/api';

// Get products
function getProducts($apiKey, $baseUrl) {
    $ch = curl_init("$baseUrl/partner/products");
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_HTTPHEADER, [
        "X-API-Key: $apiKey"
    ]);
    
    $response = curl_exec($ch);
    $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
    curl_close($ch);
    
    if ($httpCode !== 200) {
        throw new Exception("HTTP Error: $httpCode");
    }
    
    return json_decode($response, true);
}

// Create product
function createProduct($apiKey, $baseUrl, $productData) {
    $ch = curl_init("$baseUrl/partner/products");
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_POST, true);
    curl_setopt($ch, CURLOPT_HTTPHEADER, [
        "X-API-Key: $apiKey",
        "Content-Type: application/json"
    ]);
    curl_setopt($ch, CURLOPT_POSTFIELDS, json_encode($productData));
    
    $response = curl_exec($ch);
    $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
    curl_close($ch);
    
    if ($httpCode !== 201) {
        throw new Exception("HTTP Error: $httpCode - $response");
    }
    
    return json_decode($response, true);
}

// Example usage
try {
    $products = getProducts($apiKey, $baseUrl);
    echo "Found " . count($products) . " products\n";
} catch (Exception $e) {
    echo "Error: " . $e->getMessage() . "\n";
}
?>
```

## Testing with Postman

1. Import the API collection (if available)
2. Set environment variables:
   - `base_url`: https://localhost:5001/api
   - `api_key`: Your API key
   - `jwt_token`: Your JWT token (for token management)
3. Use the examples above to create requests
4. Save successful requests to your collection

## Best Practices

1. **Always use HTTPS** in production
2. **Store API keys securely** (environment variables, secret managers)
3. **Implement retry logic** with exponential backoff for network errors
4. **Handle rate limits** gracefully (429 Too Many Requests)
5. **Validate responses** before processing data
6. **Log API errors** for debugging
7. **Monitor API usage** to detect anomalies
8. **Rotate API keys** regularly
9. **Use IP whitelisting** when possible
10. **Set appropriate token expiration** dates

## Support

For API support:
- Documentation: https://docs.mercato.com/api
- Email: api-support@mercato.com
- GitHub Issues: https://github.com/mercato/api-issues
