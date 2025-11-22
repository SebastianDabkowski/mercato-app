using SD.Mercato.Notification.DTOs;

namespace SD.Mercato.Notification.Services;

/// <summary>
/// Service for sending notifications via different channels.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send a notification asynchronously.
    /// </summary>
    Task<SendNotificationResponse> SendNotificationAsync(SendNotificationRequest request);

    /// <summary>
    /// Send an email notification.
    /// </summary>
    Task<SendNotificationResponse> SendEmailNotificationAsync(
        string recipientUserId,
        string recipientEmail,
        string eventType,
        string subject,
        string templateName,
        Dictionary<string, string> templateData,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null);

    /// <summary>
    /// Retry failed notifications.
    /// </summary>
    Task<int> RetryFailedNotificationsAsync();
}
