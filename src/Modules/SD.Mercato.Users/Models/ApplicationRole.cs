using Microsoft.AspNetCore.Identity;

namespace SD.Mercato.Users.Models;

/// <summary>
/// Represents a role in the Mercato system.
/// Uses ASP.NET Core Identity roles.
/// </summary>
public class ApplicationRole : IdentityRole
{
    /// <summary>
    /// Description of the role.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Standard role names used in the system.
/// </summary>
public static class RoleNames
{
    public const string Buyer = "Buyer";
    public const string Seller = "Seller";
    public const string Administrator = "Administrator";
}
