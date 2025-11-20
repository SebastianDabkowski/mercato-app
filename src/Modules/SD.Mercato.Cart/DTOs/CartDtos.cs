using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Cart.DTOs;

/// <summary>
/// Request model for adding an item to cart.
/// </summary>
public class AddToCartRequest
{
    [Required(ErrorMessage = "Product ID is required")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}

/// <summary>
/// Request model for updating cart item quantity.
/// </summary>
public class UpdateCartItemRequest
{
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}

/// <summary>
/// Cart item data transfer object.
/// </summary>
public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string ProductTitle { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PriceAtAdd { get; set; }
    public decimal CurrentPrice { get; set; }
    public bool IsPriceChanged => PriceAtAdd != CurrentPrice;
    public int AvailableStock { get; set; }
    public bool IsAvailable { get; set; }
    public decimal Subtotal => Quantity * CurrentPrice;
    public DateTime AddedAt { get; set; }
}

/// <summary>
/// Cart data transfer object with grouped items.
/// </summary>
public class CartDto
{
    public Guid Id { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public Dictionary<Guid, List<CartItemDto>> ItemsByStore { get; set; } = new();
    public decimal TotalAmount => Items.Sum(i => i.Subtotal);
    public int TotalItems => Items.Sum(i => i.Quantity);
    public bool HasPriceChanges => Items.Any(i => i.IsPriceChanged);
    public bool HasUnavailableItems => Items.Any(i => !i.IsAvailable);
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}

/// <summary>
/// Response model for cart operations.
/// </summary>
public class CartResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public CartDto? Cart { get; set; }
}

/// <summary>
/// Response model for add to cart operation.
/// </summary>
public class AddToCartResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid? CartItemId { get; set; }
    public CartDto? Cart { get; set; }
}
