using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.ProductCatalog.Models;

/// <summary>
/// Represents a question asked by a buyer about a product.
/// </summary>
public class ProductQuestion
{
    /// <summary>
    /// Unique identifier for the question.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Product ID this question relates to.
    /// </summary>
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Navigation property to the Product.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// User ID of the person who asked the question.
    /// </summary>
    [Required]
    public string AskedByUserId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the person who asked (for display purposes).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string AskedByName { get; set; } = string.Empty;

    /// <summary>
    /// The question text.
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Status of the question: Pending, Answered, Hidden.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = ProductQuestionStatus.Pending;

    /// <summary>
    /// Whether this question is visible to the public.
    /// </summary>
    [Required]
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Collection of answers to this question.
    /// </summary>
    public List<ProductAnswer> Answers { get; set; } = new();

    /// <summary>
    /// Timestamp when the question was asked.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the question was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Product question status constants.
/// </summary>
public static class ProductQuestionStatus
{
    public const string Pending = "Pending";
    public const string Answered = "Answered";
    public const string Hidden = "Hidden";
}
