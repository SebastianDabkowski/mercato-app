using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SD.Mercato.Cart.Services;
using SD.Mercato.History.Data;
using SD.Mercato.History.DTOs;
using SD.Mercato.History.Models;
using SD.Mercato.ProductCatalog.Services;
using SD.Mercato.SellerPanel.Services;

namespace SD.Mercato.History.Services;

/// <summary>
/// Service for order management operations.
/// </summary>
public class OrderService : IOrderService
{
    private readonly HistoryDbContext _dbContext;
    private readonly ICartService _cartService;
    private readonly IProductService _productService;
    private readonly IStoreService _storeService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        HistoryDbContext dbContext,
        ICartService cartService,
        IProductService productService,
        IStoreService storeService,
        ILogger<OrderService> logger)
    {
        _dbContext = dbContext;
        _cartService = cartService;
        _productService = productService;
        _storeService = storeService;
        _logger = logger;
    }

    public async Task<CreateOrderResponse> CreateOrderFromCartAsync(string userId, CreateOrderRequest request)
    {
        try
        {
            // Get user's cart
            var cart = await _cartService.GetCartAsync(userId, null);
            if (cart == null || !cart.Items.Any())
            {
                return new CreateOrderResponse
                {
                    Success = false,
                    Message = "Cart is empty"
                };
            }

            // Validate cart items (stock, prices, availability)
            var validationErrors = new List<string>();
            foreach (var item in cart.Items)
            {
                if (!item.IsAvailable)
                {
                    validationErrors.Add($"Product '{item.ProductTitle}' is no longer available");
                }
                if (item.Quantity > item.AvailableStock)
                {
                    validationErrors.Add($"Insufficient stock for '{item.ProductTitle}'. Available: {item.AvailableStock}");
                }
                // TODO: Should we validate price changes? If yes, what's the acceptable threshold (e.g., 10%)?
            }

            if (validationErrors.Any())
            {
                return new CreateOrderResponse
                {
                    Success = false,
                    Message = "Cart validation failed: " + string.Join(", ", validationErrors)
                };
            }

            // Calculate totals by seller
            var itemsByStore = cart.ItemsByStore;
            decimal grandTotal = 0;

            // Create Order
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                BuyerEmail = request.ContactEmail,
                BuyerPhone = request.ContactPhone,
                DeliveryRecipientName = request.DeliveryRecipientName,
                DeliveryAddressLine1 = request.DeliveryAddressLine1,
                DeliveryAddressLine2 = request.DeliveryAddressLine2,
                DeliveryCity = request.DeliveryCity,
                DeliveryState = request.DeliveryState,
                DeliveryPostalCode = request.DeliveryPostalCode,
                DeliveryCountry = request.DeliveryCountry,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = OrderPaymentStatus.Pending,
                Status = OrderStatus.Pending,
                Currency = "USD",
                CreatedAt = DateTime.UtcNow
            };

            // Create SubOrders for each seller
            foreach (var (storeId, storeItems) in itemsByStore)
            {
                // Get store info
                var store = await _storeService.GetStoreByIdAsync(storeId);
                var storeName = store?.DisplayName ?? "Unknown Store";

                // Get shipping method for this store
                var shippingMethod = request.ShippingMethods.GetValueOrDefault(storeId, "Platform Managed");
                
                // Calculate shipping cost (MVP: simple flat rate)
                // TODO: Implement proper shipping cost calculation based on weight, dimensions, destination
                var shippingCost = CalculateShippingCost(shippingMethod, storeItems.Count);

                var productsTotal = storeItems.Sum(i => i.Subtotal);
                var subOrderTotal = productsTotal + shippingCost;
                grandTotal += subOrderTotal;

                var subOrder = new SubOrder
                {
                    Id = Guid.NewGuid(),
                    SubOrderNumber = GenerateSubOrderNumber(),
                    OrderId = order.Id,
                    StoreId = storeId,
                    StoreName = storeName,
                    ProductsTotal = productsTotal,
                    ShippingCost = shippingCost,
                    TotalAmount = subOrderTotal,
                    ShippingMethod = shippingMethod,
                    Status = SubOrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                // Create SubOrderItems
                foreach (var cartItem in storeItems)
                {
                    var subOrderItem = new SubOrderItem
                    {
                        Id = Guid.NewGuid(),
                        SubOrderId = subOrder.Id,
                        ProductId = cartItem.ProductId,
                        ProductSku = cartItem.ProductTitle, // TODO: Get actual SKU from product
                        ProductTitle = cartItem.ProductTitle,
                        ProductImageUrl = cartItem.ProductImageUrl,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.CurrentPrice,
                        Subtotal = cartItem.Subtotal
                    };

                    subOrder.Items.Add(subOrderItem);
                }

                order.SubOrders.Add(subOrder);
            }

            order.TotalAmount = grandTotal;

            // Save order to database
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Deduct stock for all products
            // TODO: Implement stock deduction via ProductService
            // TODO: Should this be done here or after payment confirmation?
            // Current assumption: deduct after payment (will be called from payment callback)

            // Clear cart after order creation
            // TODO: Should we clear cart immediately or after payment confirmation?
            // Current assumption: clear after payment confirmation

            _logger.LogInformation("Order created: OrderId={OrderId}, OrderNumber={OrderNumber}, UserId={UserId}",
                order.Id, order.OrderNumber, userId);

            return new CreateOrderResponse
            {
                Success = true,
                Message = "Order created successfully",
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                // TODO: Generate actual payment redirect URL from payment gateway
                PaymentRedirectUrl = $"/checkout/payment?orderId={order.Id}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for user {UserId}", userId);
            return new CreateOrderResponse
            {
                Success = false,
                Message = "An error occurred while creating the order"
            };
        }
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId, string userId)
    {
        var order = await _dbContext.Orders
            .Include(o => o.SubOrders)
            .ThenInclude(s => s.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
        {
            return null;
        }

        return MapToOrderDto(order);
    }

    public async Task<List<OrderDto>> GetUserOrdersAsync(string userId)
    {
        var orders = await _dbContext.Orders
            .Include(o => o.SubOrders)
            .ThenInclude(s => s.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToOrderDto).ToList();
    }

    public async Task<List<SubOrderDto>> GetStoreSubOrdersAsync(Guid storeId)
    {
        var subOrders = await _dbContext.SubOrders
            .Include(s => s.Items)
            .Where(s => s.StoreId == storeId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return subOrders.Select(MapToSubOrderDto).ToList();
    }

    public async Task<bool> UpdatePaymentStatusAsync(Guid orderId, string paymentStatus, string? transactionId)
    {
        var order = await _dbContext.Orders.FindAsync(orderId);
        if (order == null)
        {
            return false;
        }

        order.PaymentStatus = paymentStatus;
        order.PaymentTransactionId = transactionId;
        order.UpdatedAt = DateTime.UtcNow;

        if (paymentStatus == OrderPaymentStatus.Paid)
        {
            order.PaidAt = DateTime.UtcNow;
            order.Status = OrderStatus.Processing;

            // Update all sub-orders to Processing status
            var subOrders = await _dbContext.SubOrders
                .Where(s => s.OrderId == orderId)
                .ToListAsync();

            foreach (var subOrder in subOrders)
            {
                subOrder.Status = SubOrderStatus.Processing;
                subOrder.UpdatedAt = DateTime.UtcNow;
            }

            // TODO: Deduct stock for all items in the order
            // TODO: Clear the user's cart
            // TODO: Send order confirmation email to buyer
            // TODO: Send new order notification to sellers
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Payment status updated: OrderId={OrderId}, Status={Status}, TransactionId={TransactionId}",
            orderId, paymentStatus, transactionId ?? "null");

        return true;
    }

    public async Task<bool> MarkSubOrderAsShippedAsync(Guid subOrderId, Guid storeId, string? trackingNumber)
    {
        var subOrder = await _dbContext.SubOrders
            .FirstOrDefaultAsync(s => s.Id == subOrderId && s.StoreId == storeId);

        if (subOrder == null)
        {
            return false;
        }

        subOrder.Status = SubOrderStatus.Shipped;
        subOrder.TrackingNumber = trackingNumber;
        subOrder.ShippedAt = DateTime.UtcNow;
        subOrder.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("SubOrder marked as shipped: SubOrderId={SubOrderId}, TrackingNumber={TrackingNumber}",
            subOrderId, trackingNumber ?? "none");

        // TODO: Send shipment notification email to buyer

        return true;
    }

    private static string GenerateOrderNumber()
    {
        // Generate unique order number: MKT-YYYY-NNNNNN
        var timestamp = DateTime.UtcNow;
        var random = new Random().Next(100000, 999999);
        return $"MKT-{timestamp:yyyy}-{random:D6}";
    }

    private static string GenerateSubOrderNumber()
    {
        // Generate unique sub-order number: SUB-YYYY-NNNNNN
        var timestamp = DateTime.UtcNow;
        var random = new Random().Next(100000, 999999);
        return $"SUB-{timestamp:yyyy}-{random:D6}";
    }

    private static decimal CalculateShippingCost(string shippingMethod, int itemCount)
    {
        // MVP: Simple flat rate shipping calculation
        // TODO: Implement proper shipping cost calculation based on:
        // - Product weight and dimensions
        // - Destination address
        // - Shipping provider rates
        // - Delivery speed (standard, express, etc.)
        
        return shippingMethod.ToLower() switch
        {
            "platform managed" => 5.00m + (itemCount * 2.00m),
            "express" => 15.00m + (itemCount * 3.00m),
            _ => 5.00m + (itemCount * 2.00m)
        };
    }

    private static OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            BuyerEmail = order.BuyerEmail,
            BuyerPhone = order.BuyerPhone,
            DeliveryRecipientName = order.DeliveryRecipientName,
            DeliveryAddressLine1 = order.DeliveryAddressLine1,
            DeliveryAddressLine2 = order.DeliveryAddressLine2,
            DeliveryCity = order.DeliveryCity,
            DeliveryState = order.DeliveryState,
            DeliveryPostalCode = order.DeliveryPostalCode,
            DeliveryCountry = order.DeliveryCountry,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            PaymentStatus = order.PaymentStatus,
            PaymentMethod = order.PaymentMethod,
            Status = order.Status,
            SubOrders = order.SubOrders.Select(MapToSubOrderDto).ToList(),
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt
        };
    }

    private static SubOrderDto MapToSubOrderDto(SubOrder subOrder)
    {
        return new SubOrderDto
        {
            Id = subOrder.Id,
            SubOrderNumber = subOrder.SubOrderNumber,
            StoreId = subOrder.StoreId,
            StoreName = subOrder.StoreName,
            ProductsTotal = subOrder.ProductsTotal,
            ShippingCost = subOrder.ShippingCost,
            TotalAmount = subOrder.TotalAmount,
            ShippingMethod = subOrder.ShippingMethod,
            TrackingNumber = subOrder.TrackingNumber,
            Status = subOrder.Status,
            Items = subOrder.Items.Select(MapToSubOrderItemDto).ToList(),
            CreatedAt = subOrder.CreatedAt,
            ShippedAt = subOrder.ShippedAt,
            DeliveredAt = subOrder.DeliveredAt
        };
    }

    private static SubOrderItemDto MapToSubOrderItemDto(SubOrderItem item)
    {
        return new SubOrderItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductSku = item.ProductSku,
            ProductTitle = item.ProductTitle,
            ProductImageUrl = item.ProductImageUrl,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            Subtotal = item.Subtotal
        };
    }
}
