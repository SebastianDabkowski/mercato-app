using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.ProductCatalog.Models;

/// <summary>
/// Represents an answer to a product question, typically from the seller.
/// </summary>
public class ProductAnswer
{
    /// <summary>
    /// Unique identifier for the answer.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Question ID this answer belongs to.
    /// </summary>
    [Required]
    public Guid QuestionId { get; set; }

    /// <summary>
    /// Navigation property to the Question.
    /// </summary>
    public ProductQuestion? Question { get; set; }

    /// <summary>
    /// User ID of the person who answered (usually the seller).
    /// </summary>
    [Required]
    public string AnsweredByUserId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the person who answered (for display purposes).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string AnsweredByName { get; set; } = string.Empty;

    /// <summary>
    /// Role of the person who answered (Seller, Admin, etc.).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string AnsweredByRole { get; set; } = string.Empty;

    /// <summary>
    /// The answer text.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string AnswerText { get; set; } = string.Empty;

    /// <summary>
    /// Whether this answer is visible to the public.
    /// </summary>
    [Required]
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Timestamp when the answer was posted.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the answer was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
