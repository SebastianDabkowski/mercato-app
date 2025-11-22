namespace SD.Mercato.Notification.Services;

/// <summary>
/// Service for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send an email with rendered template.
    /// </summary>
    Task<bool> SendEmailAsync(
        string recipientEmail,
        string subject,
        string htmlBody);

    /// <summary>
    /// Render an email template with provided data.
    /// </summary>
    string RenderTemplate(string templateName, Dictionary<string, string> data);
}
