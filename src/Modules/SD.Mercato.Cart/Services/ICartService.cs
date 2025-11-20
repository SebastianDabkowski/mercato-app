using SD.Mercato.Cart.DTOs;

namespace SD.Mercato.Cart.Services;

/// <summary>
/// Service interface for shopping cart operations.
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Get the active cart for a user or session.
    /// </summary>
    /// <param name="userId">User ID for authenticated users, null for guests.</param>
    /// <param name="sessionId">Session ID for guest users, null for authenticated.</param>
    /// <returns>Cart DTO with enriched product information.</returns>
    Task<CartDto?> GetCartAsync(string? userId, string? sessionId);

    /// <summary>
    /// Add an item to the cart.
    /// </summary>
    /// <param name="userId">User ID for authenticated users, null for guests.</param>
    /// <param name="sessionId">Session ID for guest users, null for authenticated.</param>
    /// <param name="request">Add to cart request.</param>
    /// <returns>Add to cart response with updated cart.</returns>
    Task<AddToCartResponse> AddItemAsync(string? userId, string? sessionId, AddToCartRequest request);

    /// <summary>
    /// Update the quantity of a cart item.
    /// </summary>
    /// <param name="userId">User ID for authenticated users, null for guests.</param>
    /// <param name="sessionId">Session ID for guest users, null for authenticated.</param>
    /// <param name="cartItemId">Cart item ID to update.</param>
    /// <param name="request">Update request with new quantity.</param>
    /// <returns>Cart response with updated cart.</returns>
    Task<CartResponse> UpdateItemQuantityAsync(string? userId, string? sessionId, Guid cartItemId, UpdateCartItemRequest request);

    /// <summary>
    /// Remove an item from the cart.
    /// </summary>
    /// <param name="userId">User ID for authenticated users, null for guests.</param>
    /// <param name="sessionId">Session ID for guest users, null for authenticated.</param>
    /// <param name="cartItemId">Cart item ID to remove.</param>
    /// <returns>True if removed successfully.</returns>
    Task<bool> RemoveItemAsync(string? userId, string? sessionId, Guid cartItemId);

    /// <summary>
    /// Clear all items from the cart.
    /// </summary>
    /// <param name="userId">User ID for authenticated users, null for guests.</param>
    /// <param name="sessionId">Session ID for guest users, null for authenticated.</param>
    /// <returns>True if cleared successfully.</returns>
    Task<bool> ClearCartAsync(string? userId, string? sessionId);

    /// <summary>
    /// Migrate a guest cart to a user account upon login.
    /// </summary>
    /// <param name="sessionId">Session ID of the guest cart.</param>
    /// <param name="userId">User ID to migrate the cart to.</param>
    /// <returns>True if migration was successful.</returns>
    Task<bool> MigrateGuestCartAsync(string sessionId, string userId);

    /// <summary>
    /// Expire carts that haven't been updated in 30 days.
    /// </summary>
    /// <returns>Number of carts expired.</returns>
    Task<int> ExpireInactiveCartsAsync();
}
