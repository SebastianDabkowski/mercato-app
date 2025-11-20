using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Cart.Models;

/// <summary>
/// Represents a shopping cart for a user.
/// Supports multi-seller items in a unified cart experience.
/// </summary>
public class Cart
{
    /// <summary>
    /// Unique identifier for the cart.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// User ID for authenticated users (null for guest carts).
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Session ID for guest carts (null for authenticated users).
    /// </summary>
    [MaxLength(200)]
    public string? SessionId { get; set; }

    /// <summary>
    /// Collection of items in the cart.
    /// </summary>
    public List<CartItem> Items { get; set; } = new();

    /// <summary>
    /// Timestamp when the cart was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the cart was last updated.
    /// </summary>
    [Required]
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cart status: Active or Expired.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = CartStatus.Active;
}

/// <summary>
/// Represents an item in a shopping cart.
/// </summary>
public class CartItem
{
    /// <summary>
    /// Unique identifier for the cart item.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Cart ID this item belongs to.
    /// </summary>
    [Required]
    public Guid CartId { get; set; }

    /// <summary>
    /// Navigation property to Cart.
    /// </summary>
    public Cart? Cart { get; set; }

    /// <summary>
    /// Product ID (foreign key to Product in ProductCatalog module).
    /// </summary>
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Store ID (for grouping by seller).
    /// </summary>
    [Required]
    public Guid StoreId { get; set; }

    /// <summary>
    /// Quantity of this product in the cart.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    /// <summary>
    /// Price per unit at the time the item was added to cart.
    /// Used to detect price changes.
    /// </summary>
    [Required]
    public decimal PriceAtAdd { get; set; }

    /// <summary>
    /// Timestamp when the item was added to cart.
    /// </summary>
    [Required]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the item was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Cart status constants.
/// </summary>
public static class CartStatus
{
    public const string Active = "Active";
    public const string Expired = "Expired";
}
