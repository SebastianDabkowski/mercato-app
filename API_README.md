# Mercato Partner & Public APIs - Quick Start

Welcome to the Mercato Partner and Public APIs! This guide will help you get started with integrating your systems with Mercato.

## 🚀 Quick Start (5 minutes)

### Step 1: Create a Seller Account
Register at https://mercato.com/register and create your store.

### Step 2: Generate an API Token
1. Log in to your seller dashboard
2. Navigate to Settings → API Tokens
3. Click "Generate New Token"
4. Select permissions (e.g., `products:read`, `products:write`, `orders:read`)
5. **Save the token securely** - it's only shown once!

### Step 3: Make Your First API Call

```bash
# Get all your products
curl -X GET https://api.mercato.com/api/partner/products \
  -H "X-API-Key: mrc_your_api_key_here"
```

That's it! You're now ready to integrate.

## 📚 Documentation

- **[Partner API Documentation](./PARTNER_API_DOCUMENTATION.md)** - Complete API reference
- **[Usage Examples](./API_USAGE_EXAMPLES.md)** - Code examples in multiple languages
- **[Swagger UI](https://api.mercato.com/swagger)** - Interactive API explorer (development only)

## 🔑 Authentication Methods

### Public APIs (No Auth Required)
```bash
curl https://api.mercato.com/api/public/products
```

### Partner APIs (API Key)
```bash
curl https://api.mercato.com/api/partner/products \
  -H "X-API-Key: mrc_your_api_key_here"
```

### User APIs (JWT Token)
```bash
curl https://api.mercato.com/api/apitokens \
  -H "Authorization: Bearer your_jwt_token"
```

## 🎯 Common Use Cases

### 1. Sync Product Catalog
```bash
# Get all products
GET /api/partner/products

# Create new product
POST /api/partner/products

# Update product
PUT /api/partner/products/{productId}

# Update stock only
PATCH /api/partner/products/{productId}/stock
```

### 2. Monitor Orders
```bash
# Get all orders
GET /api/partner/orders

# Filter by status
GET /api/partner/orders?status=Processing

# Filter by date range
GET /api/partner/orders?fromDate=2025-11-01&toDate=2025-11-30
```

### 3. Public Product Search (for buyers)
```bash
# Search products
POST /api/public/products/search

# Get product details
GET /api/public/products/{productId}
```

## 🔐 Security Best Practices

1. ✅ **Use HTTPS** always
2. ✅ **Store API keys securely** (never commit to git)
3. ✅ **Set token expiration** dates
4. ✅ **Use IP whitelisting** when possible
5. ✅ **Rotate tokens** regularly
6. ✅ **Grant minimal permissions** needed
7. ✅ **Monitor token usage** via `lastUsedAt` field

## ⚙️ Feature Flags

Write operations can be controlled via feature flags:

```json
{
  "PartnerApi": {
    "EnableWrite": false
  }
}
```

When disabled, write endpoints return `403 Forbidden`.

## 🛠️ Integration SDKs

### Node.js / JavaScript
```javascript
const apiKey = 'mrc_your_api_key_here';

async function getProducts() {
  const res = await fetch('https://api.mercato.com/api/partner/products', {
    headers: { 'X-API-Key': apiKey }
  });
  return await res.json();
}
```

### Python
```python
import requests

headers = {'X-API-Key': 'mrc_your_api_key_here'}
response = requests.get('https://api.mercato.com/api/partner/products', headers=headers)
products = response.json()
```

### PHP
```php
$ch = curl_init('https://api.mercato.com/api/partner/products');
curl_setopt($ch, CURLOPT_HTTPHEADER, ['X-API-Key: mrc_your_api_key_here']);
curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
$products = json_decode(curl_exec($ch), true);
```

See [API_USAGE_EXAMPLES.md](./API_USAGE_EXAMPLES.md) for complete examples.

## 📊 API Endpoints Summary

### Public APIs (No auth)
- `GET /api/public/products` - List all published products
- `GET /api/public/products/{id}` - Get product details
- `GET /api/public/stores/{id}/products` - List store products
- `POST /api/public/products/search` - Search & filter products

### Partner APIs (API key required)
- `GET /api/partner/products` - List your products
- `GET /api/partner/products/{id}` - Get product details
- `POST /api/partner/products` - Create product ⚠️ Requires write enabled
- `PUT /api/partner/products/{id}` - Update product ⚠️ Requires write enabled
- `PATCH /api/partner/products/{id}/stock` - Update stock ⚠️ Requires write enabled
- `GET /api/partner/orders` - List your orders

### Token Management (JWT required)
- `POST /api/apitokens` - Create API token
- `GET /api/apitokens` - List your tokens
- `GET /api/apitokens/{id}` - Get token details
- `PUT /api/apitokens/{id}` - Update token
- `DELETE /api/apitokens/{id}` - Revoke token

## 🎨 Permissions

API tokens support granular permissions:

- **`products:read`** - Read product data
- **`products:write`** - Create/update products
- **`orders:read`** - Read order data

More permissions coming soon!

## 🐛 Error Handling

All errors follow a consistent format:

```json
{
  "message": "Error description",
  "errors": {
    "field": ["Validation error message"]
  }
}
```

**Common HTTP Status Codes:**
- `200` OK - Success
- `201` Created - Resource created
- `400` Bad Request - Validation error
- `401` Unauthorized - Missing/invalid auth
- `403` Forbidden - Insufficient permissions
- `404` Not Found - Resource not found
- `429` Too Many Requests - Rate limit exceeded
- `500` Internal Server Error - Server error

## 📈 Rate Limits

**Recommended (to be enforced):**
- Public APIs: 100 requests/minute per IP
- Partner APIs: 1000 requests/minute per API key
- User APIs: 60 requests/minute per user

Rate limiting is not currently enforced but will be added soon.

## 🔄 Webhooks (Coming Soon)

Webhook support for real-time notifications is planned:

- `order.created`
- `order.updated`
- `order.shipped`
- `product.low_stock`

## 🆘 Support & Resources

- **Documentation**: https://docs.mercato.com/api
- **Status Page**: https://status.mercato.com
- **Support Email**: api-support@mercato.com
- **Community Forum**: https://community.mercato.com

## 📝 Changelog

### v1.0.0 (2025-11-23)
- ✨ Initial release
- ✨ API token authentication
- ✨ Public read APIs
- ✨ Partner product APIs (CRUD)
- ✨ Partner order APIs (read)
- ✨ Swagger/OpenAPI documentation
- ✨ Feature flags for write operations

## 🤝 Contributing

Found a bug or have a feature request? Please open an issue on GitHub or contact our support team.

## 📄 License

API usage is subject to Mercato's Terms of Service: https://mercato.com/terms

---

**Happy Integrating! 🎉**

For detailed API reference, see [PARTNER_API_DOCUMENTATION.md](./PARTNER_API_DOCUMENTATION.md)
