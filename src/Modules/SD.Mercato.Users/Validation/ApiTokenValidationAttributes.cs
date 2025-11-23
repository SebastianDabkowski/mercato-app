using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Users.Validation;

/// <summary>
/// Validates that a list of permissions is not empty and contains only valid permission values.
/// </summary>
public class ValidPermissionsAttribute : ValidationAttribute
{
    private static readonly string[] ValidPermissions = new[]
    {
        "products:read",
        "products:write",
        "orders:read"
    };

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not List<string> permissions)
        {
            return new ValidationResult("Permissions must be a list of strings");
        }

        if (permissions.Count == 0)
        {
            return new ValidationResult("At least one permission is required");
        }

        var invalidPermissions = permissions.Where(p => !ValidPermissions.Contains(p)).ToList();
        if (invalidPermissions.Any())
        {
            return new ValidationResult($"Invalid permissions: {string.Join(", ", invalidPermissions)}. Valid values are: {string.Join(", ", ValidPermissions)}");
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Validates that a date is in the future.
/// </summary>
public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success; // Allow null values
        }

        if (value is not DateTime dateValue)
        {
            return new ValidationResult("Value must be a DateTime");
        }

        if (dateValue <= DateTime.UtcNow)
        {
            return new ValidationResult("Expiration date must be in the future");
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Validates that a comma-separated string contains valid IP addresses.
/// </summary>
public class IpAddressListAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success; // Allow null or empty values
        }

        var ipList = value.ToString()!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var invalidIps = new List<string>();
        foreach (var ip in ipList)
        {
            if (!System.Net.IPAddress.TryParse(ip, out _))
            {
                invalidIps.Add(ip);
            }
        }

        if (invalidIps.Any())
        {
            return new ValidationResult($"Invalid IP addresses: {string.Join(", ", invalidIps)}");
        }

        return ValidationResult.Success;
    }
}
