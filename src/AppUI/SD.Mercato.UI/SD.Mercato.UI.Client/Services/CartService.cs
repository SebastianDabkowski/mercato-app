using System.Net.Http.Json;

namespace SD.Mercato.UI.Client.Services;

/// <summary>
/// DTOs for cart operations (client-side).
/// </summary>
public class AddToCartRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}

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

public class CartResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public CartDto? Cart { get; set; }
}

public class AddToCartResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Guid? CartItemId { get; set; }
    public CartDto? Cart { get; set; }
}

/// <summary>
/// Interface for cart service.
/// </summary>
public interface ICartService
{
    Task<CartDto?> GetCartAsync();
    Task<AddToCartResponse> AddItemAsync(AddToCartRequest request);
    Task<CartResponse> UpdateItemQuantityAsync(Guid cartItemId, UpdateCartItemRequest request);
    Task<bool> RemoveItemAsync(Guid cartItemId);
    Task<bool> ClearCartAsync();
    event Action? OnCartChanged;
}

/// <summary>
/// Cart service for Blazor WebAssembly client.
/// Manages cart state and session handling.
/// </summary>
public class CartService : ICartService
{
    private readonly HttpClient _httpClient;
    private string? _sessionId;

    public event Action? OnCartChanged;

    public CartService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        // Load session ID from local storage if available
        _sessionId = GetOrCreateSessionId();
    }

    public async Task<CartDto?> GetCartAsync()
    {
        try
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "api/cart");
            AddSessionHeader(requestMessage);
            
            var response = await _httpClient.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                UpdateSessionIdFromResponse(response);
                return await response.Content.ReadFromJsonAsync<CartDto>();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting cart: {ex.Message}");
            return null;
        }
    }

    public async Task<AddToCartResponse> AddItemAsync(AddToCartRequest request)
    {
        try
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "api/cart/items")
            {
                Content = JsonContent.Create(request)
            };
            AddSessionHeader(requestMessage);
            
            var response = await _httpClient.SendAsync(requestMessage);
            
            // Update session ID from response headers if provided
            UpdateSessionIdFromResponse(response);
            
            var result = await response.Content.ReadFromJsonAsync<AddToCartResponse>();
            
            if (result?.Success == true)
            {
                OnCartChanged?.Invoke();
            }
            
            return result ?? new AddToCartResponse { Success = false, Message = "Unknown error occurred" };
        }
        catch (Exception ex)
        {
            // Log error for debugging
            Console.Error.WriteLine($"Error adding item to cart: {ex.Message}");
            return new AddToCartResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<CartResponse> UpdateItemQuantityAsync(Guid cartItemId, UpdateCartItemRequest request)
    {
        try
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"api/cart/items/{cartItemId}")
            {
                Content = JsonContent.Create(request)
            };
            AddSessionHeader(requestMessage);
            
            var response = await _httpClient.SendAsync(requestMessage);
            var result = await response.Content.ReadFromJsonAsync<CartResponse>();
            
            if (result?.Success == true)
            {
                OnCartChanged?.Invoke();
            }
            
            return result ?? new CartResponse { Success = false, Message = "Unknown error occurred" };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating cart item: {ex.Message}");
            return new CartResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<bool> RemoveItemAsync(Guid cartItemId)
    {
        try
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"api/cart/items/{cartItemId}");
            AddSessionHeader(requestMessage);
            
            var response = await _httpClient.SendAsync(requestMessage);
            
            if (response.IsSuccessStatusCode)
            {
                OnCartChanged?.Invoke();
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error removing cart item: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ClearCartAsync()
    {
        try
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "api/cart");
            AddSessionHeader(requestMessage);
            
            var response = await _httpClient.SendAsync(requestMessage);
            
            if (response.IsSuccessStatusCode)
            {
                OnCartChanged?.Invoke();
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error clearing cart: {ex.Message}");
            return false;
        }
    }

    private string GetOrCreateSessionId()
    {
        // In a real app, this would use localStorage
        // For now, just generate a new one
        return Guid.NewGuid().ToString();
    }

    private void AddSessionHeader(HttpRequestMessage request)
    {
        if (_sessionId != null)
        {
            request.Headers.Add("X-Session-Id", _sessionId);
        }
    }

    private void UpdateSessionIdFromResponse(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("X-Session-Id", out var values))
        {
            var newSessionId = values.FirstOrDefault();
            if (!string.IsNullOrEmpty(newSessionId))
            {
                _sessionId = newSessionId;
                // In a real app, save to localStorage
            }
        }
    }
}
