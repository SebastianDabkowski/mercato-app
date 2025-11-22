using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SD.Mercato.Cart.Services;
using SD.Mercato.History.Data;
using SD.Mercato.History.DTOs;
using SD.Mercato.History.Models;
using SD.Mercato.ProductCatalog.Services;
using SD.Mercato.SellerPanel.Services;
using SD.Mercato.Notification.Services;
using SD.Mercato.Notification.Models;

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
    private readonly INotificationService? _notificationService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        HistoryDbContext dbContext,
        ICartService cartService,
        IProductService productService,
        IStoreService storeService,
        ILogger<OrderService> logger,
        INotificationService? notificationService = null)
    {
        _dbContext = dbContext;
        _cartService = cartService;
        _productService = productService;
        _storeService = storeService;
        _logger = logger;
        _notificationService = notificationService;
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

            // Batch load all required data upfront to avoid N+1 queries
            var storeIds = itemsByStore.Keys.ToList();
            var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();

            var stores = await _storeService.GetStoresByIdsAsync(storeIds);
            var products = await _productService.GetProductsByIdsAsync(productIds);

            // Create lookup dictionaries for O(1) access
            var storeDict = stores.ToDictionary(s => s.Id);
            var productDict = products.ToDictionary(p => p.Id);

            // Create SubOrders for each seller
            foreach (var (storeId, storeItems) in itemsByStore)
            {
                // Get store info from dictionary
                var storeName = storeDict.TryGetValue(storeId, out var store) 
                    ? store.DisplayName 
                    : "Unknown Store";

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
                    // Get product SKU from dictionary
                    var productSku = productDict.TryGetValue(cartItem.ProductId, out var product)
                        ? product.SKU
                        : cartItem.ProductId.ToString();

                    var subOrderItem = new SubOrderItem
                    {
                        Id = Guid.NewGuid(),
                        SubOrderId = subOrder.Id,
                        ProductId = cartItem.ProductId,
                        ProductSku = productSku,
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

    public async Task<OrderListResponse> GetUserOrdersAsync(string userId, OrderFilterRequest filter)
    {
        var query = _dbContext.Orders
            .Include(o => o.SubOrders)
            .ThenInclude(s => s.Items)
            .Where(o => o.UserId == userId);

        // Apply filters
        if (!string.IsNullOrEmpty(filter.Status))
        {
            query = query.Where(o => o.Status == filter.Status);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            // Include entire day (use < next day for robust filtering)
            var nextDay = filter.ToDate.Value.Date.AddDays(1);
            query = query.Where(o => o.CreatedAt < nextDay);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply ordering and pagination
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new OrderListResponse
        {
            Orders = orders.Select(MapToOrderDto).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
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

    public async Task<SubOrderListResponse> GetStoreSubOrdersAsync(Guid storeId, SubOrderFilterRequest filter)
    {
        var query = _dbContext.SubOrders
            .Include(s => s.Items)
            .Include(s => s.Order) // Include parent order for delivery address
            .Where(s => s.StoreId == storeId);

        // Apply filters
        if (!string.IsNullOrEmpty(filter.Status))
        {
            query = query.Where(s => s.Status == filter.Status);
        }

        if (filter.FromDate.HasValue)
        {
            query = query.Where(s => s.CreatedAt >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            // Include entire day (use < next day for robust filtering)
            var nextDay = filter.ToDate.Value.Date.AddDays(1);
            query = query.Where(s => s.CreatedAt < nextDay);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply ordering and pagination
        var subOrders = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new SubOrderListResponse
        {
            SubOrders = subOrders.Select(MapToSubOrderDto).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<SubOrderDto?> GetSubOrderByIdAsync(Guid subOrderId, Guid storeId)
    {
        var subOrder = await _dbContext.SubOrders
            .Include(s => s.Items)
            .Include(s => s.Order) // Include parent order for delivery address
            .FirstOrDefaultAsync(s => s.Id == subOrderId && s.StoreId == storeId);

        if (subOrder == null)
        {
            return null;
        }

        return MapToSubOrderDto(subOrder);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateSubOrderStatusAsync(
        Guid subOrderId, 
        Guid storeId, 
        UpdateSubOrderStatusRequest request)
    {
        var subOrder = await _dbContext.SubOrders
            .Include(s => s.Order)
                .ThenInclude(o => o!.SubOrders)
            .FirstOrDefaultAsync(s => s.Id == subOrderId && s.StoreId == storeId);

        if (subOrder == null)
        {
            return (false, "SubOrder not found");
        }

        // Validate status transition
        var validationResult = ValidateStatusTransition(subOrder.Status, request.Status);
        if (!validationResult.IsValid)
        {
            return (false, validationResult.ErrorMessage);
        }

        // Validate tracking number for shipped status
        if (request.Status == SubOrderStatus.Shipped && string.IsNullOrWhiteSpace(request.TrackingNumber))
        {
            // TODO: Should tracking number be mandatory when marking as shipped?
            // Current assumption: recommended but not mandatory
            _logger.LogWarning("SubOrder {SubOrderId} marked as shipped without tracking number", subOrderId);
        }

        // Update SubOrder status
        var oldStatus = subOrder.Status;
        subOrder.Status = request.Status;
        subOrder.UpdatedAt = DateTime.UtcNow;

        // Set specific timestamp fields based on status
        if (request.Status == SubOrderStatus.Shipped)
        {
            subOrder.ShippedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(request.TrackingNumber))
            {
                subOrder.TrackingNumber = request.TrackingNumber;
            }
            if (!string.IsNullOrWhiteSpace(request.CarrierName))
            {
                subOrder.CarrierName = request.CarrierName;
            }
        }
        else if (request.Status == SubOrderStatus.Delivered)
        {
            subOrder.DeliveredAt = DateTime.UtcNow;
        }

        // Check if all SubOrders are completed to update parent Order status
        var allSubOrders = subOrder.Order!.SubOrders;

        // Determine parent order status
        if (allSubOrders.All(s => s.Status == SubOrderStatus.Delivered))
        {
            subOrder.Order.Status = OrderStatus.Completed;
            subOrder.Order.UpdatedAt = DateTime.UtcNow;
        }
        else if (allSubOrders.All(s => s.Status == SubOrderStatus.Cancelled))
        {
            subOrder.Order.Status = OrderStatus.Cancelled;
            subOrder.Order.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "SubOrder status updated: SubOrderId={SubOrderId}, OldStatus={OldStatus}, NewStatus={NewStatus}, StoreId={StoreId}",
            subOrderId, oldStatus, request.Status, storeId);

        // Send notification to buyer about status change
        if (_notificationService != null && subOrder.Order != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var statusMessage = request.Status switch
                    {
                        SubOrderStatus.Shipped => "has been shipped",
                        SubOrderStatus.Delivered => "has been delivered",
                        SubOrderStatus.Cancelled => "has been cancelled",
                        _ => $"status has been updated to {request.Status}"
                    };

                    await _notificationService.SendEmailNotificationAsync(
                        recipientUserId: subOrder.Order.UserId,
                        recipientEmail: subOrder.Order.BuyerEmail,
                        eventType: NotificationEventTypes.OrderStatusChanged,
                        subject: $"Order Status Update - {subOrder.SubOrderNumber}",
                        templateName: "OrderStatusChanged",
                        templateData: new Dictionary<string, string>
                        {
                            { "CustomerName", subOrder.Order.DeliveryRecipientName },
                            { "OrderNumber", subOrder.SubOrderNumber },
                            { "NewStatus", request.Status },
                            { "TrackingNumber", request.TrackingNumber ?? "Not provided" }
                        },
                        relatedEntityId: subOrder.Id,
                        relatedEntityType: "SubOrder"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send order status notification for SubOrderId={SubOrderId}", subOrderId);
                }
            });
        }
        
        // TODO: Trigger payout calculation when status is Delivered

        return (true, null);
    }

    /// <summary>
    /// Validates if a status transition is allowed based on the workflow.
    /// Workflow: Processing → Shipped → Delivered
    /// Cancellation allowed from Processing or Shipped (with restrictions)
    /// </summary>
    private static (bool IsValid, string? ErrorMessage) ValidateStatusTransition(string currentStatus, string newStatus)
    {
        // Allow setting to same status (no-op)
        if (currentStatus == newStatus)
        {
            return (true, null);
        }

        // Define valid transitions based on requirements
        // Note: The requirement mentions "New → In preparation → Shipped → Completed → Canceled"
        // but the existing code uses "Processing" instead of "New/In preparation"
        // TODO: Should we introduce separate "New" and "InPreparation" statuses or keep "Processing"?
        // Current assumption: "Processing" covers both "New" and "In Preparation"
        
        var validTransitions = new Dictionary<string, List<string>>
        {
            [SubOrderStatus.Pending] = new() { SubOrderStatus.Processing, SubOrderStatus.Cancelled },
            [SubOrderStatus.Processing] = new() { SubOrderStatus.Shipped, SubOrderStatus.Cancelled },
            [SubOrderStatus.Shipped] = new() { SubOrderStatus.Delivered, SubOrderStatus.Cancelled },
            [SubOrderStatus.Delivered] = new() { }, // Terminal state - no transitions
            [SubOrderStatus.Cancelled] = new() { }  // Terminal state - no transitions
        };

        if (!validTransitions.TryGetValue(currentStatus, out var allowedTransitions))
        {
            return (false, $"Invalid current status: {currentStatus}");
        }

        if (!allowedTransitions.Contains(newStatus))
        {
            return (false, $"Cannot transition from {currentStatus} to {newStatus}. Allowed transitions: {string.Join(", ", allowedTransitions)}");
        }

        return (true, null);
    }

    public async Task<bool> UpdatePaymentStatusAsync(Guid orderId, string paymentStatus, string? transactionId)
    {
        // Validate payment status
        var validStatuses = new[] 
        { 
            OrderPaymentStatus.Pending, 
            OrderPaymentStatus.Paid, 
            OrderPaymentStatus.Failed, 
            OrderPaymentStatus.Refunded 
        };

        if (!validStatuses.Contains(paymentStatus))
        {
            _logger.LogWarning("Invalid payment status attempted: {PaymentStatus} for OrderId={OrderId}", 
                paymentStatus, orderId);
            return false;
        }

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
            
            // Send order confirmation email to buyer
            if (_notificationService != null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _notificationService.SendEmailNotificationAsync(
                            recipientUserId: order.UserId,
                            recipientEmail: order.BuyerEmail,
                            eventType: NotificationEventTypes.OrderCreated,
                            subject: $"Order Confirmation - {order.OrderNumber}",
                            templateName: "OrderConfirmation",
                            templateData: new Dictionary<string, string>
                            {
                                { "CustomerName", order.DeliveryRecipientName },
                                { "OrderNumber", order.OrderNumber },
                                { "OrderDate", order.CreatedAt.ToString("MMMM dd, yyyy") },
                                { "TotalAmount", order.TotalAmount.ToString("F2") }
                            },
                            relatedEntityId: order.Id,
                            relatedEntityType: "Order"
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send order confirmation email for OrderId={OrderId}", orderId);
                    }
                });

                // Send new order notification to each seller
                foreach (var subOrder in subOrders)
                {
                    var storeId = subOrder.StoreId;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // Get store owner details
                            var store = await _storeService.GetStoreByIdAsync(storeId);
                            if (store != null && !string.IsNullOrEmpty(store.ContactEmail))
                            {
                                await _notificationService.SendEmailNotificationAsync(
                                    recipientUserId: store.OwnerUserId,
                                    recipientEmail: store.ContactEmail,
                                    eventType: NotificationEventTypes.OrderCreated,
                                    subject: $"New Order Received - {subOrder.SubOrderNumber}",
                                    templateName: "NewOrderSeller",
                                    templateData: new Dictionary<string, string>
                                    {
                                        { "SellerName", store.DisplayName },
                                        { "SubOrderNumber", subOrder.SubOrderNumber },
                                        { "OrderDate", subOrder.CreatedAt.ToString("MMMM dd, yyyy") },
                                        { "Amount", subOrder.TotalAmount.ToString("F2") },
                                        { "ItemCount", subOrder.Items.Count.ToString() }
                                    },
                                    relatedEntityId: subOrder.Id,
                                    relatedEntityType: "SubOrder"
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send seller notification for SubOrderId={SubOrderId}", subOrder.Id);
                        }
                    });
                }
            }
        }
        else if (paymentStatus == OrderPaymentStatus.Failed || paymentStatus == OrderPaymentStatus.Refunded)
        {
            // Send payment status notification to buyer
            if (_notificationService != null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _notificationService.SendEmailNotificationAsync(
                            recipientUserId: order.UserId,
                            recipientEmail: order.BuyerEmail,
                            eventType: NotificationEventTypes.PaymentStatusChanged,
                            subject: $"Payment {paymentStatus} - {order.OrderNumber}",
                            templateName: "PaymentStatusChanged",
                            templateData: new Dictionary<string, string>
                            {
                                { "CustomerName", order.DeliveryRecipientName },
                                { "OrderNumber", order.OrderNumber },
                                { "PaymentStatus", paymentStatus },
                                { "Amount", order.TotalAmount.ToString("F2") }
                            },
                            relatedEntityId: order.Id,
                            relatedEntityType: "Order"
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send payment status email for OrderId={OrderId}", orderId);
                    }
                });
            }
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

    public Task<CalculateShippingResponse> CalculateShippingCostsAsync(CalculateShippingRequest request)
    {
        var response = new CalculateShippingResponse();
        decimal totalShipping = 0;

        foreach (var (storeId, selection) in request.ShippingMethods)
        {
            var cost = CalculateShippingCost(selection.Method, selection.ItemCount);
            response.ShippingCostsByStore[storeId] = cost;
            totalShipping += cost;
        }

        response.TotalShippingCost = totalShipping;
        return Task.FromResult(response);
    }

    private static string GenerateOrderNumber()
    {
        // Generate unique order number: MKT-YYYYMMDD-NNNNNNNNNN
        // Using timestamp (date) + longer GUID part for better uniqueness
        var timestamp = DateTime.UtcNow;
        var guidPart = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
        return $"MKT-{timestamp:yyyyMMdd}-{guidPart}";
    }

    private static string GenerateSubOrderNumber()
    {
        // Generate unique sub-order number: SUB-YYYYMMDD-NNNNNNNNNN
        // Using timestamp (date) + longer GUID part for better uniqueness
        var timestamp = DateTime.UtcNow;
        var guidPart = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
        return $"SUB-{timestamp:yyyyMMdd}-{guidPart}";
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
            OrderId = subOrder.OrderId,
            OrderNumber = subOrder.Order?.OrderNumber ?? string.Empty,
            StoreId = subOrder.StoreId,
            StoreName = subOrder.StoreName,
            ProductsTotal = subOrder.ProductsTotal,
            ShippingCost = subOrder.ShippingCost,
            TotalAmount = subOrder.TotalAmount,
            ShippingMethod = subOrder.ShippingMethod,
            TrackingNumber = subOrder.TrackingNumber,
            CarrierName = subOrder.CarrierName,
            Status = subOrder.Status,
            Items = subOrder.Items.Select(MapToSubOrderItemDto).ToList(),
            CreatedAt = subOrder.CreatedAt,
            ShippedAt = subOrder.ShippedAt,
            DeliveredAt = subOrder.DeliveredAt,
            // Include delivery information from parent Order (for seller view)
            DeliveryRecipientName = subOrder.Order?.DeliveryRecipientName,
            DeliveryAddressLine1 = subOrder.Order?.DeliveryAddressLine1,
            DeliveryAddressLine2 = subOrder.Order?.DeliveryAddressLine2,
            DeliveryCity = subOrder.Order?.DeliveryCity,
            DeliveryState = subOrder.Order?.DeliveryState,
            DeliveryPostalCode = subOrder.Order?.DeliveryPostalCode,
            DeliveryCountry = subOrder.Order?.DeliveryCountry,
            BuyerEmail = subOrder.Order?.BuyerEmail,
            BuyerPhone = subOrder.Order?.BuyerPhone
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
