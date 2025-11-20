# Shopping Cart Implementation Guide

## Overview

The shopping cart module enables buyers to add products from multiple sellers into a unified cart experience. The system internally groups items by seller while presenting a seamless single cart to the buyer.

## Architecture

### Backend Components

#### Models (`SD.Mercato.Cart.Models`)
- **Cart**: Represents a shopping cart (user or guest session)
- **CartItem**: Represents individual products in the cart

#### Database Schema (`cart` schema)
- **Carts** table: Stores cart metadata
  - `Id` (PK): Unique cart identifier
  - `UserId`: For authenticated users (nullable)
  - `SessionId`: For guest users (nullable)
  - `Status`: Active or Expired
  - `CreatedAt`, `LastUpdatedAt`: Timestamps

- **CartItems** table: Stores individual cart items
  - `Id` (PK): Unique item identifier
  - `CartId` (FK): References Cart
  - `ProductId` (FK): References Product
  - `StoreId`: For grouping by seller
  - `Quantity`: Item quantity
  - `PriceAtAdd`: Price when added (for change detection)
  - `AddedAt`, `UpdatedAt`: Timestamps

#### Services (`SD.Mercato.Cart.Services`)
- **ICartService / CartService**: Core cart operations
  - Get cart
  - Add item
  - Update quantity
  - Remove item
  - Clear cart
  - Migrate guest cart to user
  - Expire inactive carts

#### API Endpoints (`/api/cart`)
- `GET /api/cart` - Get current cart
- `POST /api/cart/items` - Add item to cart
- `PUT /api/cart/items/{itemId}` - Update item quantity
- `DELETE /api/cart/items/{itemId}` - Remove item
- `DELETE /api/cart` - Clear entire cart
- `POST /api/cart/migrate` - Migrate guest cart to user account

### Frontend Components

#### Services (`SD.Mercato.UI.Client.Services`)
- **ICartService / CartService**: Client-side cart management
  - Handles session ID management
  - Communicates with API
  - Fires cart changed events

#### Pages
- **Cart.razor**: Main cart page
  - Displays grouped items by seller
  - Shows price changes and availability
  - Quantity management
  - Remove items
  - Clear cart
  - Order summary

## Key Features

### Multi-Seller Support
- Cart items are automatically grouped by `StoreId` (seller)
- Each seller's items displayed in separate card
- Total calculated across all sellers

### Price Change Detection
- Price at add time (`PriceAtAdd`) stored with each item
- Current price fetched on cart load
- Visual indicator when prices differ
- User sees both old and new prices

### Stock Validation
- Stock checked when adding items
- Stock availability displayed on cart page
- Prevents checkout if items unavailable
- Warning when quantity exceeds available stock

### Guest Cart Support
- Session ID stored in `X-Session-Id` HTTP header
- Session-based carts for unauthenticated users
- Automatic migration to user account on login

### Cart Expiration
- Carts expire after 30 days of inactivity
- Background job can mark expired carts (ExpireInactiveCartsAsync)

### Business Rules
- Maximum 50 unique items per cart
- Minimum quantity: 1
- Quantity cannot exceed available stock
- Products must be "Published" status
- Price changes shown but don't block checkout

## Usage Examples

### Adding to Cart (API)

```http
POST /api/cart/items
Content-Type: application/json
X-Session-Id: guest-session-123

{
  "productId": "550e8400-e29b-41d4-a716-446655440000",
  "quantity": 2
}
```

Response:
```json
{
  "success": true,
  "message": "Item added to cart successfully",
  "cartItemId": "660e8400-e29b-41d4-a716-446655440000",
  "cart": {
    "id": "770e8400-e29b-41d4-a716-446655440000",
    "items": [...],
    "itemsByStore": {...},
    "totalAmount": 49.98,
    "totalItems": 2
  }
}
```

### Getting Cart (API)

```http
GET /api/cart
X-Session-Id: guest-session-123
```

Response includes:
- All cart items with enriched product data
- Items grouped by store
- Price change indicators
- Stock availability
- Total amount and item count

### Migrating Guest Cart (API)

After login, call migration endpoint:

```http
POST /api/cart/migrate
Authorization: Bearer {token}
Content-Type: application/json

{
  "sessionId": "guest-session-123"
}
```

This merges guest cart items into the user's account cart.

## Database Migrations

To apply cart migrations:

```bash
cd src/Modules/SD.Mercato.Cart
dotnet ef database update --startup-project ../../API/SD.Mercato.API --context CartDbContext
```

## Frontend Integration

### Registering the Service

In `Program.cs`:
```csharp
builder.Services.AddScoped<ICartService, CartService>();
```

### Using in Components

```razor
@inject ICartService CartService

<button @onclick="AddToCart">Add to Cart</button>

@code {
    private async Task AddToCart()
    {
        var result = await CartService.AddItemAsync(new AddToCartRequest
        {
            ProductId = productId,
            Quantity = 1
        });
        
        if (result.Success)
        {
            // Show success message
        }
    }
}
```

### Listening to Cart Changes

```csharp
protected override async Task OnInitializedAsync()
{
    CartService.OnCartChanged += OnCartChanged;
}

private void OnCartChanged()
{
    InvokeAsync(StateHasChanged);
}

public void Dispose()
{
    CartService.OnCartChanged -= OnCartChanged;
}
```

## Testing Checklist

- [ ] Add item to cart (authenticated user)
- [ ] Add item to cart (guest user)
- [ ] Add multiple items from different sellers
- [ ] Update item quantity
- [ ] Remove item from cart
- [ ] Clear entire cart
- [ ] View cart with seller grouping
- [ ] Price change detection
- [ ] Stock validation (insufficient stock)
- [ ] Product becomes unavailable
- [ ] Max 50 items limit
- [ ] Guest cart migration on login
- [ ] Session persistence across page refreshes
- [ ] Cart expiration after 30 days

## Future Enhancements

- [ ] Cart item notes/customization
- [ ] Save for later functionality
- [ ] Recommended products in cart
- [ ] Apply discount codes
- [ ] Estimate shipping costs
- [ ] Wishlist integration
- [ ] Cart abandonment tracking
- [ ] Email reminders for abandoned carts

## Troubleshooting

### Cart Not Persisting
- Check that `X-Session-Id` header is being sent
- Verify session ID is stored in client
- Check database connection

### Price Changes Not Detected
- Ensure `PriceAtAdd` is set when adding items
- Verify product service returns current price
- Check `EnrichCartDtoAsync` logic

### Guest Cart Not Migrating
- Verify session ID is passed to migration endpoint
- Check authentication token is valid
- Ensure migration endpoint is called after login

## Performance Considerations

- Cart loading enriches each item with product/store data
- Consider caching store names to reduce queries
- Index on `CartId`, `ProductId`, `UserId`, and `SessionId`
- Expired carts should be cleaned up periodically

## Security Notes

- Session IDs should be cryptographically random
- Guest carts tied to session, not cookies (for security)
- Cart migration requires authentication
- Validate product availability and stock server-side
- Price changes calculated server-side (never trust client)
