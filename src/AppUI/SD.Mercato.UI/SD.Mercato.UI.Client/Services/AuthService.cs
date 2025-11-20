using Blazored.LocalStorage;
using SD.Mercato.UI.Client.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SD.Mercato.UI.Client.Services;

/// <summary>
/// Interface for authentication service.
/// </summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<UserDto?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetTokenAsync();
}

/// <summary>
/// Authentication service for Blazor WebAssembly client.
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private const string TokenKey = "authToken";
    private const string UserKey = "currentUser";

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (result?.Success == true && result.Token != null)
            {
                await StoreAuthDataAsync(result.Token, result.User);
            }

            return result ?? new AuthResponse { Success = false, Message = "Unknown error occurred" };
        }
        catch (Exception ex)
        {
            return new AuthResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (result?.Success == true && result.Token != null)
            {
                await StoreAuthDataAsync(result.Token, result.User);
            }

            return result ?? new AuthResponse { Success = false, Message = "Unknown error occurred" };
        }
        catch (Exception ex)
        {
            return new AuthResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                await _httpClient.PostAsync("api/auth/logout", null);
            }
        }
        catch (HttpRequestException ex)
        {
            // Ignore HTTP errors during logout API call
            Console.Error.WriteLine($"HTTP error during logout: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            // Ignore cancellation errors during logout API call
            Console.Error.WriteLine($"Timeout during logout: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Log unexpected errors during logout API call
            Console.Error.WriteLine($"Unexpected error during logout: {ex}");
        }
        finally
        {
            await ClearAuthDataAsync();
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        return await _localStorage.GetItemAsync<UserDto>(UserKey);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsStringAsync(TokenKey);
    }

    private async Task StoreAuthDataAsync(string token, UserDto? user)
    {
        await _localStorage.SetItemAsStringAsync(TokenKey, token);
        if (user != null)
        {
            await _localStorage.SetItemAsync(UserKey, user);
        }
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task ClearAuthDataAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        await _localStorage.RemoveItemAsync(UserKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
