using Microsoft.EntityFrameworkCore;
using SD.Mercato.Users.Data;
using SD.Mercato.Users.DTOs;
using SD.Mercato.Users.Models;
using System.Security.Cryptography;

namespace SD.Mercato.Users.Services;

/// <summary>
/// Implementation of API token management service.
/// </summary>
public class ApiTokenService : IApiTokenService
{
    private readonly UsersDbContext _context;
    private const string TokenPrefix = "mrc_"; // Mercato API token prefix

    public ApiTokenService(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<CreateApiTokenResponse> CreateTokenAsync(string userId, Guid? storeId, CreateApiTokenRequest request)
    {
        // Generate a cryptographically secure random token
        var tokenValue = GenerateSecureToken();
        var tokenHash = HashToken(tokenValue);

        var apiToken = new ApiToken
        {
            Id = Guid.NewGuid(),
            TokenHash = tokenHash,
            Name = request.Name,
            UserId = userId,
            StoreId = storeId,
            Permissions = string.Join(",", request.Permissions),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
            IsActive = true,
            IpWhitelist = request.IpWhitelist,
            Notes = request.Notes
        };

        _context.ApiTokens.Add(apiToken);
        await _context.SaveChangesAsync();

        return new CreateApiTokenResponse
        {
            Id = apiToken.Id,
            Token = TokenPrefix + tokenValue, // Return the actual token only at creation
            Name = apiToken.Name,
            Permissions = request.Permissions,
            CreatedAt = apiToken.CreatedAt,
            ExpiresAt = apiToken.ExpiresAt
        };
    }

    public async Task<(bool IsValid, Guid? TokenId, string? UserId, Guid? StoreId, List<string>? Permissions)> ValidateTokenAsync(string token, string? ipAddress = null)
    {
        // Remove prefix if present
        if (token.StartsWith(TokenPrefix))
        {
            token = token.Substring(TokenPrefix.Length);
        }

        var tokenHash = HashToken(token);

        var apiToken = await _context.ApiTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.IsActive);

        if (apiToken == null)
        {
            return (false, null, null, null, null);
        }

        // Check expiration
        if (apiToken.ExpiresAt.HasValue && apiToken.ExpiresAt.Value < DateTime.UtcNow)
        {
            return (false, null, null, null, null);
        }

        // Check IP whitelist if configured
        if (!string.IsNullOrEmpty(apiToken.IpWhitelist) && !string.IsNullOrEmpty(ipAddress))
        {
            var allowedIps = apiToken.IpWhitelist.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (!allowedIps.Contains(ipAddress))
            {
                return (false, null, null, null, null);
            }
        }

        var permissions = apiToken.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        return (true, apiToken.Id, apiToken.UserId, apiToken.StoreId, permissions);
    }

    public async Task<List<ApiTokenDto>> GetUserTokensAsync(string userId)
    {
        var tokens = await _context.ApiTokens
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return tokens.Select(MapToDto).ToList();
    }

    public async Task<ApiTokenDto?> GetTokenByIdAsync(Guid tokenId, string userId)
    {
        var token = await _context.ApiTokens
            .FirstOrDefaultAsync(t => t.Id == tokenId && t.UserId == userId);

        return token != null ? MapToDto(token) : null;
    }

    public async Task<bool> UpdateTokenAsync(Guid tokenId, string userId, UpdateApiTokenRequest request)
    {
        var token = await _context.ApiTokens
            .FirstOrDefaultAsync(t => t.Id == tokenId && t.UserId == userId);

        if (token == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            token.Name = request.Name;
        }

        if (request.IsActive.HasValue)
        {
            token.IsActive = request.IsActive.Value;
        }

        if (request.Notes != null)
        {
            token.Notes = request.Notes;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTokenAsync(Guid tokenId, string userId)
    {
        var token = await _context.ApiTokens
            .FirstOrDefaultAsync(t => t.Id == tokenId && t.UserId == userId);

        if (token == null)
        {
            return false;
        }

        _context.ApiTokens.Remove(token);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task RecordTokenUsageAsync(Guid tokenId)
    {
        var token = await _context.ApiTokens.FindAsync(tokenId);
        if (token != null)
        {
            // TODO: Optimize token usage tracking for high load scenarios
            // Consider implementing a background service or batching strategy
            // to update LastUsedAt less frequently (e.g., once per hour per token)
            token.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Generate a cryptographically secure random token (64 characters).
    /// Uses URL-safe Base64 encoding for better compatibility.
    /// </summary>
    private static string GenerateSecureToken()
    {
        var bytes = new byte[48]; // 48 bytes = 384 bits, gives us 64 base64 chars
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        // Use URL-safe Base64 encoding
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }

    /// <summary>
    /// Hash a token using SHA256 for secure storage.
    /// </summary>
    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Parse permissions from comma-separated string.
    /// </summary>
    private static List<string> ParsePermissions(string permissions)
    {
        return permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    /// <summary>
    /// Map ApiToken entity to DTO.
    /// </summary>
    private static ApiTokenDto MapToDto(ApiToken token)
    {
        return new ApiTokenDto
        {
            Id = token.Id,
            Name = token.Name,
            Permissions = ParsePermissions(token.Permissions),
            StoreId = token.StoreId,
            CreatedAt = token.CreatedAt,
            ExpiresAt = token.ExpiresAt,
            LastUsedAt = token.LastUsedAt,
            IsActive = token.IsActive,
            IpWhitelist = token.IpWhitelist,
            TokenPreview = $"{TokenPrefix}***...***" // Don't expose actual token
        };
    }
}
