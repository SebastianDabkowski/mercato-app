using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.History.DTOs;

/// <summary>
/// Request model for creating an order from checkout.
/// </summary>
public class CreateOrderRequest
{
    [Required(ErrorMessage = "Delivery recipient name is required")]
    [MaxLength(200)]
    public string DeliveryRecipientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string ContactEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact phone is required")]
    [Phone(ErrorMessage = "Invalid phone format")]
    public string ContactPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address line 1 is required")]
    [MaxLength(200)]
    public string DeliveryAddressLine1 { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DeliveryAddressLine2 { get; set; }

    [Required(ErrorMessage = "City is required")]
    [MaxLength(100)]
    public string DeliveryCity { get; set; } = string.Empty;

    [Required(ErrorMessage = "State is required")]
    [MaxLength(100)]
    public string DeliveryState { get; set; } = string.Empty;

    [Required(ErrorMessage = "Postal code is required")]
    [MaxLength(20)]
    public string DeliveryPostalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country is required")]
    [MaxLength(100)]
    public string DeliveryCountry { get; set; } = "United States";

    [Required(ErrorMessage = "Payment method is required")]
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Shipping method selection per seller.
    /// Key: StoreId, Value: Shipping method name.
    /// </summary>
    public Dictionary<Guid, string> ShippingMethods { get; set; } = new();
}

/// <summary>
/// Response model for order creation.
/// </summary>
public class CreateOrderResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public string? PaymentRedirectUrl { get; set; }
}

/// <summary>
/// Order data transfer object.
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string BuyerPhone { get; set; } = string.Empty;
    public string DeliveryRecipientName { get; set; } = string.Empty;
    public string DeliveryAddressLine1 { get; set; } = string.Empty;
    public string? DeliveryAddressLine2 { get; set; }
    public string DeliveryCity { get; set; } = string.Empty;
    public string DeliveryState { get; set; } = string.Empty;
    public string DeliveryPostalCode { get; set; } = string.Empty;
    public string DeliveryCountry { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<SubOrderDto> SubOrders { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

/// <summary>
/// SubOrder data transfer object.
/// </summary>
public class SubOrderDto
{
    public Guid Id { get; set; }
    public string SubOrderNumber { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public decimal ProductsTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingMethod { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? CarrierName { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<SubOrderItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    
    // Delivery information (for seller view)
    public string? DeliveryRecipientName { get; set; }
    public string? DeliveryAddressLine1 { get; set; }
    public string? DeliveryAddressLine2 { get; set; }
    public string? DeliveryCity { get; set; }
    public string? DeliveryState { get; set; }
    public string? DeliveryPostalCode { get; set; }
    public string? DeliveryCountry { get; set; }
    public string? BuyerEmail { get; set; }
    public string? BuyerPhone { get; set; }
}

/// <summary>
/// SubOrderItem data transfer object.
/// </summary>
public class SubOrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductTitle { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

/// <summary>
/// Response model for validating checkout.
/// </summary>
public class ValidateCheckoutResponse
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public CheckoutSummaryDto? Summary { get; set; }
}

/// <summary>
/// Checkout summary data transfer object.
/// </summary>
public class CheckoutSummaryDto
{
    public int TotalItems { get; set; }
    public decimal ProductsTotal { get; set; }
    public decimal ShippingTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public List<SellerGroupDto> SellerGroups { get; set; } = new();
}

/// <summary>
/// Seller group in checkout summary.
/// </summary>
public class SellerGroupDto
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal ProductsSubtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public List<string> AvailableShippingMethods { get; set; } = new();
}

/// <summary>
/// Request model for calculating shipping costs.
/// </summary>
public class CalculateShippingRequest
{
    public Dictionary<Guid, ShippingMethodSelection> ShippingMethods { get; set; } = new();
}

/// <summary>
/// Shipping method selection for a store.
/// </summary>
public class ShippingMethodSelection
{
    public string Method { get; set; } = "Platform Managed";
    public int ItemCount { get; set; }
}

/// <summary>
/// Response model for shipping cost calculation.
/// </summary>
public class CalculateShippingResponse
{
    public Dictionary<Guid, decimal> ShippingCostsByStore { get; set; } = new();
    public decimal TotalShippingCost { get; set; }
}

/// <summary>
/// Request model for filtering SubOrders (seller side).
/// </summary>
public class SubOrderFilterRequest
{
    /// <summary>
    /// Filter by SubOrder status (e.g., "Processing", "Shipped").
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by orders created on or after this date.
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter by orders created on or before this date.
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Page number for pagination (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Request model for updating SubOrder status.
/// </summary>
public class UpdateSubOrderStatusRequest
{
    /// <summary>
    /// New status for the SubOrder.
    /// </summary>
    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Tracking number (recommended when status is "Shipped").
    /// </summary>
    [MaxLength(200)]
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// Carrier name (e.g., "UPS", "FedEx", "USPS") - optional when status is "Shipped".
    /// </summary>
    [MaxLength(100)]
    public string? CarrierName { get; set; }

    /// <summary>
    /// Optional notes for the status update.
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// Response model for SubOrder list with pagination info.
/// </summary>
public class SubOrderListResponse
{
    public List<SubOrderDto> SubOrders { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

/// <summary>
/// Request model for filtering Orders (buyer side).
/// </summary>
public class OrderFilterRequest
{
    /// <summary>
    /// Filter by Order status.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by orders created on or after this date.
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter by orders created on or before this date.
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Page number for pagination (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Response model for Order list with pagination info.
/// </summary>
public class OrderListResponse
{
    public List<OrderDto> Orders { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}
