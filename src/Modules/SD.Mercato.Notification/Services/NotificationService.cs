using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SD.Mercato.Notification.Data;
using SD.Mercato.Notification.DTOs;
using SD.Mercato.Notification.Models;

namespace SD.Mercato.Notification.Services;

/// <summary>
/// Notification service implementation.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly NotificationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        NotificationDbContext context,
        IEmailService emailService,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Send a notification.
    /// </summary>
    public async Task<SendNotificationResponse> SendNotificationAsync(SendNotificationRequest request)
    {
        return await SendEmailNotificationAsync(
            request.RecipientUserId,
            request.RecipientEmail,
            request.EventType,
            request.Subject,
            request.TemplateName,
            request.TemplateData,
            request.RelatedEntityId,
            request.RelatedEntityType);
    }

    /// <summary>
    /// Send an email notification.
    /// </summary>
    public async Task<SendNotificationResponse> SendEmailNotificationAsync(
        string recipientUserId,
        string recipientEmail,
        string eventType,
        string subject,
        string templateName,
        Dictionary<string, string> templateData,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null)
    {
        var notificationId = Guid.NewGuid();

        try
        {
            // Render the email template
            var htmlBody = _emailService.RenderTemplate(templateName, templateData);

            // Create notification log entry
            var notificationLog = new NotificationLog
            {
                Id = notificationId,
                NotificationType = NotificationTypes.Email,
                EventType = eventType,
                RecipientUserId = recipientUserId,
                RecipientEmail = recipientEmail,
                Subject = subject,
                Message = htmlBody,
                Status = NotificationStatus.Pending,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType,
                CreatedAt = DateTime.UtcNow
            };

            _context.NotificationLogs.Add(notificationLog);
            await _context.SaveChangesAsync();

            // Send the email
            var success = await _emailService.SendEmailAsync(recipientEmail, subject, htmlBody);

            // Update notification status
            if (success)
            {
                notificationLog.Status = NotificationStatus.Sent;
                notificationLog.SentAt = DateTime.UtcNow;
                _logger.LogInformation(
                    "Notification {NotificationId} sent successfully to {RecipientEmail}",
                    notificationId,
                    recipientEmail);
            }
            else
            {
                notificationLog.Status = NotificationStatus.Failed;
                notificationLog.ErrorMessage = "Failed to send email";
                _logger.LogWarning(
                    "Notification {NotificationId} failed to send to {RecipientEmail}",
                    notificationId,
                    recipientEmail);
            }

            await _context.SaveChangesAsync();

            return new SendNotificationResponse
            {
                NotificationId = notificationId,
                Success = success,
                ErrorMessage = success ? null : "Failed to send email"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification {NotificationId}", notificationId);

            // Try to update notification log if it exists
            var log = await _context.NotificationLogs.FindAsync(notificationId);
            if (log != null)
            {
                log.Status = NotificationStatus.Failed;
                log.ErrorMessage = ex.Message;
                await _context.SaveChangesAsync();
            }

            return new SendNotificationResponse
            {
                NotificationId = notificationId,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Retry failed notifications.
    /// </summary>
    public async Task<int> RetryFailedNotificationsAsync()
    {
        var failedNotifications = await _context.NotificationLogs
            .Where(n => n.Status == NotificationStatus.Failed && n.RetryCount < 3)
            .ToListAsync();

        var retryCount = 0;

        foreach (var notification in failedNotifications)
        {
            try
            {
                // Only retry email notifications for now
                if (notification.NotificationType == NotificationTypes.Email &&
                    !string.IsNullOrEmpty(notification.RecipientEmail) &&
                    !string.IsNullOrEmpty(notification.Subject))
                {
                    var success = await _emailService.SendEmailAsync(
                        notification.RecipientEmail,
                        notification.Subject,
                        notification.Message);

                    notification.RetryCount++;

                    if (success)
                    {
                        notification.Status = NotificationStatus.Sent;
                        notification.SentAt = DateTime.UtcNow;
                        notification.ErrorMessage = null;
                        retryCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retry notification {NotificationId}", notification.Id);
                notification.ErrorMessage = ex.Message;
            }
        }

        await _context.SaveChangesAsync();

        return retryCount;
    }
}
