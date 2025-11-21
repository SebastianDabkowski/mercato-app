using System.Net.Http.Json;
using System.Text.Json;

namespace SD.Mercato.UI.Client.Services;

/// <summary>
/// DTOs for order operations (client-side).
/// </summary>
public class CreateOrderRequest
{
    public string DeliveryRecipientName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string DeliveryAddressLine1 { get; set; } = string.Empty;
    public string? DeliveryAddressLine2 { get; set; }
    public string DeliveryCity { get; set; } = string.Empty;
    public string DeliveryState { get; set; } = string.Empty;
    public string DeliveryPostalCode { get; set; } = string.Empty;
    public string DeliveryCountry { get; set; } = "United States";
    public string PaymentMethod { get; set; } = string.Empty;
    public Dictionary<Guid, string> ShippingMethods { get; set; } = new();
}

public class CreateOrderResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public string? PaymentRedirectUrl { get; set; }
}

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
/// Service for order operations.
/// </summary>
public interface IOrderService
{
    Task<CreateOrderResponse?> CreateOrderAsync(CreateOrderRequest request);
    Task<List<OrderDto>?> GetOrdersAsync();
    Task<OrderListResponse?> GetOrdersAsync(
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20);
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId);
    Task<CalculateShippingResponse?> CalculateShippingAsync(CalculateShippingRequest request);
}

public class OrderService : IOrderService
{
    private readonly HttpClient _httpClient;

    public OrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CreateOrderResponse?> CreateOrderAsync(CreateOrderRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/checkout/create-order", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Create order failed: {response.StatusCode} - {errorContent}");
            
            return new CreateOrderResponse
            {
                Success = false,
                Message = $"Failed to create order: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating order: {ex.Message}");
            return new CreateOrderResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<List<OrderDto>?> GetOrdersAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<OrderDto>>("/api/orders");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting orders: {ex.Message}");
            return null;
        }
    }

    public async Task<OrderListResponse?> GetOrdersAsync(
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(status))
                queryParams.Add($"status={Uri.EscapeDataString(status)}");
            
            if (fromDate.HasValue)
                queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            
            if (toDate.HasValue)
                queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
            
            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");
            
            var queryString = string.Join("&", queryParams);
            var url = $"/api/orders?{queryString}";
            
            return await _httpClient.GetFromJsonAsync<OrderListResponse>(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting orders with filters: {ex.Message}");
            return null;
        }
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<OrderDto>($"/api/orders/{orderId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting order: {ex.Message}");
            return null;
        }
    }

    public async Task<CalculateShippingResponse?> CalculateShippingAsync(CalculateShippingRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/checkout/calculate-shipping", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CalculateShippingResponse>();
            }

            Console.WriteLine($"Calculate shipping failed: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating shipping: {ex.Message}");
            return null;
        }
    }
}
