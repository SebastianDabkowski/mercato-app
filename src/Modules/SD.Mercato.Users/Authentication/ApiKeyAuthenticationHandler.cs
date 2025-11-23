using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SD.Mercato.Users.Services;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SD.Mercato.Users.Authentication;

/// <summary>
/// Authentication handler for API key-based authentication.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IApiTokenService _apiTokenService;
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiTokenService apiTokenService)
        : base(options, logger, encoder)
    {
        _apiTokenService = apiTokenService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if API key header is present
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var apiKey = apiKeyHeaderValues.FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey))
        {
            return AuthenticateResult.NoResult();
        }

        // Get client IP address for validation
        var ipAddress = Context.Connection.RemoteIpAddress?.ToString();

        // Validate the API key
        var (isValid, tokenId, userId, storeId, permissions) = await _apiTokenService.ValidateTokenAsync(apiKey, ipAddress);

        if (!isValid || userId == null || tokenId == null)
        {
            return AuthenticateResult.Fail("Invalid API key");
        }

        // Record token usage (async fire-and-forget to avoid blocking the request)
        _ = Task.Run(async () =>
        {
            try
            {
                await _apiTokenService.RecordTokenUsageAsync(tokenId.Value);
            }
            catch
            {
                // Ignore errors in token usage tracking to avoid breaking authentication
            }
        });

        // Create claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("AuthenticationType", "ApiKey"),
            new Claim("TokenId", tokenId.Value.ToString())
        };

        if (storeId.HasValue)
        {
            claims.Add(new Claim("StoreId", storeId.Value.ToString()));
        }

        // Add permission claims
        if (permissions != null)
        {
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}

/// <summary>
/// Constants for API key authentication scheme.
/// </summary>
public static class ApiKeyAuthenticationDefaults
{
    public const string AuthenticationScheme = "ApiKey";
}
