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
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public decimal ProductsTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingMethod { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<SubOrderItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
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
/// Service for order operations.
/// </summary>
public interface IOrderService
{
    Task<CreateOrderResponse?> CreateOrderAsync(CreateOrderRequest request);
    Task<List<OrderDto>?> GetOrdersAsync();
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId);
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
}
