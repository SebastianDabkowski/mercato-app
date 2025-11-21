using SD.Mercato.History.DTOs;

namespace SD.Mercato.History.Services;

/// <summary>
/// Service interface for order management operations.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Create an order from the user's cart.
    /// </summary>
    Task<CreateOrderResponse> CreateOrderFromCartAsync(string userId, CreateOrderRequest request);

    /// <summary>
    /// Get order by ID.
    /// </summary>
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId, string userId);

    /// <summary>
    /// Get all orders for a user.
    /// </summary>
    Task<List<OrderDto>> GetUserOrdersAsync(string userId);

    /// <summary>
    /// Get sub-orders for a seller's store.
    /// </summary>
    Task<List<SubOrderDto>> GetStoreSubOrdersAsync(Guid storeId);

    /// <summary>
    /// Update payment status after payment gateway callback.
    /// </summary>
    Task<bool> UpdatePaymentStatusAsync(Guid orderId, string paymentStatus, string? transactionId);

    /// <summary>
    /// Mark sub-order as shipped by seller.
    /// </summary>
    Task<bool> MarkSubOrderAsShippedAsync(Guid subOrderId, Guid storeId, string? trackingNumber);

    /// <summary>
    /// Calculate shipping costs for cart items grouped by store.
    /// </summary>
    Task<CalculateShippingResponse> CalculateShippingCostsAsync(CalculateShippingRequest request);
}
