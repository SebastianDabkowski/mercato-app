using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.ProductCatalog.Validation;

/// <summary>
/// Validation attribute to ensure all strings in a list are valid URLs.
/// </summary>
public class UrlListAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success; // Null is valid (use [Required] for null checks)
        }

        if (value is not List<string> urls)
        {
            return new ValidationResult("Value must be a list of strings");
        }

        var urlAttribute = new UrlAttribute();
        
        for (int i = 0; i < urls.Count; i++)
        {
            if (!urlAttribute.IsValid(urls[i]))
            {
                return new ValidationResult($"Invalid URL format at index {i}: {urls[i]}");
            }
        }

        return ValidationResult.Success;
    }
}
