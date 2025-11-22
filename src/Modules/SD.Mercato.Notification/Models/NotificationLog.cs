using System.ComponentModel.DataAnnotations;

namespace SD.Mercato.Notification.Models;

/// <summary>
/// Represents a notification log entry for tracking all notifications sent.
/// </summary>
public class NotificationLog
{
    /// <summary>
    /// Unique identifier for the notification log entry.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Type of notification (Email, Push, SMS, etc.).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string NotificationType { get; set; } = NotificationTypes.Email;

    /// <summary>
    /// Event that triggered this notification.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// User ID of the recipient.
    /// </summary>
    [Required]
    public string RecipientUserId { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the recipient (for email notifications).
    /// </summary>
    [MaxLength(255)]
    public string? RecipientEmail { get; set; }

    /// <summary>
    /// Subject line (for email notifications).
    /// </summary>
    [MaxLength(500)]
    public string? Subject { get; set; }

    /// <summary>
    /// Notification message body or template.
    /// </summary>
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Status of the notification: Pending, Sent, Failed.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = NotificationStatus.Pending;

    /// <summary>
    /// Error message if notification failed.
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Related entity ID (e.g., OrderId, CaseId, ProductId).
    /// </summary>
    public Guid? RelatedEntityId { get; set; }

    /// <summary>
    /// Related entity type (e.g., "Order", "Case", "Product").
    /// </summary>
    [MaxLength(100)]
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// Number of retry attempts for failed notifications.
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Timestamp when the notification was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the notification was sent successfully.
    /// </summary>
    public DateTime? SentAt { get; set; }
}

/// <summary>
/// Notification type constants.
/// </summary>
public static class NotificationTypes
{
    public const string Email = "Email";
    public const string Push = "Push";
    public const string SMS = "SMS";
    public const string InApp = "InApp";
}

/// <summary>
/// Notification status constants.
/// </summary>
public static class NotificationStatus
{
    public const string Pending = "Pending";
    public const string Sent = "Sent";
    public const string Failed = "Failed";
}

/// <summary>
/// Event type constants for notifications.
/// </summary>
public static class NotificationEventTypes
{
    public const string OrderCreated = "OrderCreated";
    public const string OrderStatusChanged = "OrderStatusChanged";
    public const string PaymentStatusChanged = "PaymentStatusChanged";
    public const string SubOrderShipped = "SubOrderShipped";
    public const string CaseCreated = "CaseCreated";
    public const string CaseMessageReceived = "CaseMessageReceived";
    public const string PayoutProcessed = "PayoutProcessed";
}
