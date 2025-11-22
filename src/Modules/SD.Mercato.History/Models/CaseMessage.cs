using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.History.Models;

/// <summary>
/// Represents a message or response in a customer service case.
/// Used for communication between buyer, seller, and admin.
/// </summary>
public class CaseMessage
{
    /// <summary>
    /// Unique identifier for the message.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Case ID this message belongs to.
    /// </summary>
    [Required]
    public Guid CaseId { get; set; }

    /// <summary>
    /// Navigation property to the Case.
    /// </summary>
    public Case? Case { get; set; }

    /// <summary>
    /// User ID of the person who sent this message.
    /// </summary>
    [Required]
    public string SenderId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the sender at the time of sending (for audit trail).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// Role of the sender: Buyer, Seller, Admin.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SenderRole { get; set; } = string.Empty;

    /// <summary>
    /// Message content.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the message was sent.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
