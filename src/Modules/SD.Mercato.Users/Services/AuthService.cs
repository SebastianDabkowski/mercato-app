using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SD.Mercato.Users.DTOs;
using SD.Mercato.Users.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SD.Mercato.Users.Services;

/// <summary>
/// Implementation of authentication services.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Validate role
        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            return new AuthResponse
            {
                Success = false,
                Message = $"Invalid role: {request.Role}"
            };
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "User with this email already exists"
            };
        }

        // Create new user
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new AuthResponse
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        // Assign role
        await _userManager.AddToRoleAsync(user, request.Role);

        // TODO: Send email verification
        // For MVP, we'll auto-verify emails. In production, send verification email.
        user.IsEmailVerified = true;
        await _userManager.UpdateAsync(user);

        // Generate token
        var token = GenerateJwtToken(user, request.Role);

        return new AuthResponse
        {
            Success = true,
            Token = token,
            Message = "Registration successful",
            User = MapToUserDto(user, request.Role)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Account is locked due to multiple failed login attempts. Please try again later."
                };
            }
            
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        // TODO: Should inactive users be able to log in, or return a 403 Forbidden?
        // Current assumption: all users can log in if they have valid credentials

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? RoleNames.Buyer;

        // Generate token
        var token = GenerateJwtToken(user, role);

        return new AuthResponse
        {
            Success = true,
            Token = token,
            Message = "Login successful",
            User = MapToUserDto(user, role)
        };
    }

    public async Task<AuthResponse> ExternalLoginAsync(ExternalLoginRequest request)
    {
        // Validate role
        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            return new AuthResponse
            {
                Success = false,
                Message = $"Invalid role: {request.Role}"
            };
        }

        // TODO: Verify the external provider token with the provider's API
        // For MVP, we trust the token. In production, verify with Google/Facebook API.

        // Check if user already exists
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        if (user == null)
        {
            // Create new user
            user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                IsEmailVerified = true, // External providers verify emails
                ExternalProvider = request.Provider,
                ExternalProviderId = request.ExternalProviderId,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            // Assign role
            await _userManager.AddToRoleAsync(user, request.Role);
        }
        else
        {
            // User exists - verify this is not an account takeover attempt
            // If user already has an external provider set, only allow login from the same provider
            if (!string.IsNullOrEmpty(user.ExternalProvider) && user.ExternalProvider != request.Provider)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"This account is already linked to {user.ExternalProvider}. Please use {user.ExternalProvider} to sign in."
                };
            }
            
            // If user doesn't have an external provider yet (email/password account), link it
            if (string.IsNullOrEmpty(user.ExternalProvider))
            {
                user.ExternalProvider = request.Provider;
                user.ExternalProviderId = request.ExternalProviderId;
                user.IsEmailVerified = true; // External providers verify emails
                await _userManager.UpdateAsync(user);
            }
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? request.Role;

        // Generate token
        var token = GenerateJwtToken(user, role);

        return new AuthResponse
        {
            Success = true,
            Token = token,
            Message = "Login successful",
            User = MapToUserDto(user, role)
        };
    }

    public async Task LogoutAsync(string userId)
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<UserDto?> GetUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? RoleNames.Buyer;

        return MapToUserDto(user, role);
    }

    public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    private string GenerateJwtToken(ApplicationUser user, string role)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var issuer = jwtSettings["Issuer"] ?? "MercatoAPI";
        var audience = jwtSettings["Audience"] ?? "MercatoClient";
        var expiryInMinutes = int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private UserDto MapToUserDto(ApplicationUser user, string role)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Role = role,
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            ExternalProvider = user.ExternalProvider
        };
    }
}
