using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SD.Mercato.Cart.Data;
using SD.Mercato.Cart.DTOs;
using SD.Mercato.Cart.Models;
using SD.Mercato.ProductCatalog.Services;
using SD.Mercato.SellerPanel.Services;

namespace SD.Mercato.Cart.Services;

/// <summary>
/// Service implementation for shopping cart operations.
/// </summary>
public class CartService : ICartService
{
    private const int MaxCartItems = 50;
    private const int CartExpirationDays = 30;

    private readonly CartDbContext _context;
    private readonly IProductService _productService;
    private readonly IStoreService _storeService;
    private readonly ILogger<CartService> _logger;

    public CartService(
        CartDbContext context,
        IProductService productService,
        IStoreService storeService,
        ILogger<CartService> logger)
    {
        _context = context;
        _productService = productService;
        _storeService = storeService;
        _logger = logger;
    }

    public async Task<CartDto?> GetCartAsync(string? userId, string? sessionId)
    {
        var cart = await GetOrCreateCartAsync(userId, sessionId);
        if (cart == null)
        {
            return null;
        }

        return await EnrichCartDtoAsync(cart);
    }

    public async Task<AddToCartResponse> AddItemAsync(string? userId, string? sessionId, AddToCartRequest request)
    {
        // Validate product exists and is available
        var product = await _productService.GetProductByIdAsync(request.ProductId);
        if (product == null)
        {
            return new AddToCartResponse
            {
                Success = false,
                Message = "Product not found"
            };
        }

        if (product.Status != "Published")
        {
            return new AddToCartResponse
            {
                Success = false,
                Message = "Product is not available for purchase"
            };
        }

        if (product.StockQuantity < request.Quantity)
        {
            return new AddToCartResponse
            {
                Success = false,
                Message = $"Insufficient stock. Only {product.StockQuantity} items available"
            };
        }

        // Get or create cart
        var cart = await GetOrCreateCartAsync(userId, sessionId);
        if (cart == null)
        {
            return new AddToCartResponse
            {
                Success = false,
                Message = "Failed to create cart"
            };
        }

        // Check max items limit
        if (cart.Items.Count >= MaxCartItems)
        {
            return new AddToCartResponse
            {
                Success = false,
                Message = $"Cart cannot contain more than {MaxCartItems} unique items"
            };
        }

        // Check if product already in cart
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (existingItem != null)
        {
            // Update quantity
            var newQuantity = existingItem.Quantity + request.Quantity;
            if (newQuantity > product.StockQuantity)
            {
                return new AddToCartResponse
                {
                    Success = false,
                    Message = $"Cannot add {request.Quantity} more items. Only {product.StockQuantity - existingItem.Quantity} additional items available"
                };
            }

            existingItem.Quantity = newQuantity;
            existingItem.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Add new item
            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = request.ProductId,
                StoreId = product.StoreId,
                Quantity = request.Quantity,
                PriceAtAdd = product.Price,
                AddedAt = DateTime.UtcNow
            };

            cart.Items.Add(cartItem);
        }

        cart.LastUpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Item added to cart: ProductId={ProductId}, Quantity={Quantity}, CartId={CartId}",
            request.ProductId, request.Quantity, cart.Id);

        var cartDto = await EnrichCartDtoAsync(cart);
        return new AddToCartResponse
        {
            Success = true,
            Message = "Item added to cart successfully",
            CartItemId = existingItem?.Id ?? cart.Items.Last().Id,
            Cart = cartDto
        };
    }

    public async Task<CartResponse> UpdateItemQuantityAsync(string? userId, string? sessionId, Guid cartItemId, UpdateCartItemRequest request)
    {
        var cart = await GetCartByUserOrSessionAsync(userId, sessionId);
        if (cart == null)
        {
            return new CartResponse
            {
                Success = false,
                Message = "Cart not found"
            };
        }

        var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (cartItem == null)
        {
            return new CartResponse
            {
                Success = false,
                Message = "Cart item not found"
            };
        }

        // Validate product stock
        var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
        if (product == null)
        {
            return new CartResponse
            {
                Success = false,
                Message = "Product no longer exists"
            };
        }

        if (request.Quantity > product.StockQuantity)
        {
            return new CartResponse
            {
                Success = false,
                Message = $"Insufficient stock. Only {product.StockQuantity} items available"
            };
        }

        cartItem.Quantity = request.Quantity;
        cartItem.UpdatedAt = DateTime.UtcNow;
        cart.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Cart item quantity updated: CartItemId={CartItemId}, NewQuantity={Quantity}",
            cartItemId, request.Quantity);

        var cartDto = await EnrichCartDtoAsync(cart);
        return new CartResponse
        {
            Success = true,
            Message = "Cart item updated successfully",
            Cart = cartDto
        };
    }

    public async Task<bool> RemoveItemAsync(string? userId, string? sessionId, Guid cartItemId)
    {
        var cart = await GetCartByUserOrSessionAsync(userId, sessionId);
        if (cart == null)
        {
            return false;
        }

        var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (cartItem == null)
        {
            return false;
        }

        cart.Items.Remove(cartItem);
        cart.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Cart item removed: CartItemId={CartItemId}, CartId={CartId}",
            cartItemId, cart.Id);

        return true;
    }

    public async Task<bool> ClearCartAsync(string? userId, string? sessionId)
    {
        var cart = await GetCartByUserOrSessionAsync(userId, sessionId);
        if (cart == null)
        {
            return false;
        }

        cart.Items.Clear();
        cart.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Cart cleared: CartId={CartId}", cart.Id);

        return true;
    }

    public async Task<bool> MigrateGuestCartAsync(string sessionId, string userId)
    {
        // Get guest cart
        var guestCart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.Status == CartStatus.Active);

        if (guestCart == null || !guestCart.Items.Any())
        {
            return true; // Nothing to migrate
        }

        // Get or create user cart
        var userCart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active);

        if (userCart == null)
        {
            // Simply assign the guest cart to the user
            guestCart.UserId = userId;
            guestCart.SessionId = null;
            guestCart.LastUpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Guest cart migrated to user: SessionId={SessionId}, UserId={UserId}",
                sessionId, userId);

            return true;
        }

        // Merge guest cart items into user cart
        foreach (var guestItem in guestCart.Items)
        {
            var existingItem = userCart.Items.FirstOrDefault(i => i.ProductId == guestItem.ProductId);
            if (existingItem != null)
            {
                // Combine quantities
                existingItem.Quantity += guestItem.Quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
            }
            else if (userCart.Items.Count < MaxCartItems)
            {
                // Add as new item
                guestItem.CartId = userCart.Id;
                userCart.Items.Add(guestItem);
            }
            // If max items reached, skip remaining items
        }

        // Mark guest cart as expired
        guestCart.Status = CartStatus.Expired;
        userCart.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Guest cart merged into user cart: SessionId={SessionId}, UserId={UserId}",
            sessionId, userId);

        return true;
    }

    public async Task<int> ExpireInactiveCartsAsync()
    {
        var expirationDate = DateTime.UtcNow.AddDays(-CartExpirationDays);

        var expiredCarts = await _context.Carts
            .Where(c => c.Status == CartStatus.Active && c.LastUpdatedAt < expirationDate)
            .ToListAsync();

        foreach (var cart in expiredCarts)
        {
            cart.Status = CartStatus.Expired;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Expired {Count} inactive carts", expiredCarts.Count);

        return expiredCarts.Count;
    }

    private async Task<Models.Cart?> GetCartByUserOrSessionAsync(string? userId, string? sessionId)
    {
        if (!string.IsNullOrEmpty(userId))
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active);
        }

        if (!string.IsNullOrEmpty(sessionId))
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.Status == CartStatus.Active);
        }

        return null;
    }

    private async Task<Models.Cart?> GetOrCreateCartAsync(string? userId, string? sessionId)
    {
        var cart = await GetCartByUserOrSessionAsync(userId, sessionId);

        if (cart == null)
        {
            cart = new Models.Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SessionId = sessionId,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                Status = CartStatus.Active
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return cart;
    }

    private async Task<CartDto> EnrichCartDtoAsync(Models.Cart cart)
    {
        var cartDto = new CartDto
        {
            Id = cart.Id,
            CreatedAt = cart.CreatedAt,
            LastUpdatedAt = cart.LastUpdatedAt,
            Items = new List<CartItemDto>(),
            ItemsByStore = new Dictionary<Guid, List<CartItemDto>>()
        };

        foreach (var item in cart.Items)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            if (product == null)
            {
                continue; // Skip items with deleted products
            }

            var store = await _storeService.GetStoreByIdAsync(item.StoreId);

            var cartItemDto = new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                StoreId = item.StoreId,
                StoreName = store?.DisplayName ?? store?.StoreName ?? "Unknown Store",
                ProductTitle = product.Title,
                ProductImageUrl = product.ImageUrls.FirstOrDefault() ?? string.Empty,
                Quantity = item.Quantity,
                PriceAtAdd = item.PriceAtAdd,
                CurrentPrice = product.Price,
                AvailableStock = product.StockQuantity,
                IsAvailable = product.Status == "Published" && product.StockQuantity >= item.Quantity,
                AddedAt = item.AddedAt
            };

            cartDto.Items.Add(cartItemDto);

            // Group by store
            if (!cartDto.ItemsByStore.ContainsKey(item.StoreId))
            {
                cartDto.ItemsByStore[item.StoreId] = new List<CartItemDto>();
            }
            cartDto.ItemsByStore[item.StoreId].Add(cartItemDto);
        }

        return cartDto;
    }
}
