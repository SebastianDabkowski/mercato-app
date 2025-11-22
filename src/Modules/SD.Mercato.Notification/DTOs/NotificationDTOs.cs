namespace SD.Mercato.Notification.DTOs;

/// <summary>
/// Request to send a notification.
/// </summary>
public record SendNotificationRequest
{
    public required string RecipientUserId { get; init; }
    public required string RecipientEmail { get; init; }
    public required string EventType { get; init; }
    public required string Subject { get; init; }
    public required string TemplateName { get; init; }
    public required Dictionary<string, string> TemplateData { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public string? RelatedEntityType { get; init; }
}

/// <summary>
/// Response after sending a notification.
/// </summary>
public record SendNotificationResponse
{
    public required Guid NotificationId { get; init; }
    public required bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Email template data for rendering.
/// </summary>
public record EmailTemplateData
{
    public required string TemplateName { get; init; }
    public required Dictionary<string, string> Data { get; init; }
}
