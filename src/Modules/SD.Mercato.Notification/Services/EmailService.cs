using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SD.Mercato.Notification.Services;

/// <summary>
/// Email service implementation.
/// In MVP, this logs emails to console. 
/// In production, integrate with SendGrid, AWS SES, or SMTP.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Send an email.
    /// </summary>
    public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string htmlBody)
    {
        try
        {
            // TODO: Integrate with actual email provider (SendGrid, AWS SES, SMTP)
            // For MVP, we'll log the email instead
            _logger.LogInformation(
                "Email sent to {Recipient}: {Subject}\n{Body}",
                recipientEmail,
                subject,
                htmlBody);

            // Simulate email sending delay
            await Task.Delay(100);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}", recipientEmail);
            return false;
        }
    }

    /// <summary>
    /// Render an email template with provided data.
    /// </summary>
    public string RenderTemplate(string templateName, Dictionary<string, string> data)
    {
        // Get the template content
        var template = GetTemplate(templateName);

        // Replace placeholders with actual data
        foreach (var kvp in data)
        {
            template = template.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }

        return template;
    }

    /// <summary>
    /// Get email template by name.
    /// </summary>
    private string GetTemplate(string templateName)
    {
        // In production, load from files or database
        // For MVP, we use embedded templates
        return templateName switch
        {
            "OrderConfirmation" => GetOrderConfirmationTemplate(),
            "NewOrderSeller" => GetNewOrderSellerTemplate(),
            "OrderStatusChanged" => GetOrderStatusChangedTemplate(),
            "PaymentStatusChanged" => GetPaymentStatusChangedTemplate(),
            "CaseCreated" => GetCaseCreatedTemplate(),
            "CaseMessageReceived" => GetCaseMessageReceivedTemplate(),
            _ => GetDefaultTemplate()
        };
    }

    private string GetOrderConfirmationTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .header { background-color: #4CAF50; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; }
        .footer { background-color: #f4f4f4; padding: 10px; text-align: center; font-size: 12px; }
        .order-details { background-color: #f9f9f9; padding: 15px; margin: 15px 0; border-left: 4px solid #4CAF50; }
    </style>
</head>
<body>
    <div class='header'>
        <h1>Order Confirmation</h1>
    </div>
    <div class='content'>
        <p>Dear {{CustomerName}},</p>
        <p>Thank you for your order! We've received your order and are processing it.</p>
        <div class='order-details'>
            <h3>Order Details</h3>
            <p><strong>Order Number:</strong> {{OrderNumber}}</p>
            <p><strong>Order Date:</strong> {{OrderDate}}</p>
            <p><strong>Total Amount:</strong> ${{TotalAmount}}</p>
        </div>
        <p>You can track your order status in your account dashboard.</p>
        <p>Best regards,<br>Mercato Team</p>
    </div>
    <div class='footer'>
        <p>&copy; 2024 Mercato. All rights reserved.</p>
    </div>
</body>
</html>";
    }

    private string GetNewOrderSellerTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .header { background-color: #2196F3; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; }
        .footer { background-color: #f4f4f4; padding: 10px; text-align: center; font-size: 12px; }
        .order-details { background-color: #f9f9f9; padding: 15px; margin: 15px 0; border-left: 4px solid #2196F3; }
    </style>
</head>
<body>
    <div class='header'>
        <h1>New Order Received</h1>
    </div>
    <div class='content'>
        <p>Hello {{SellerName}},</p>
        <p>You have received a new order for your store!</p>
        <div class='order-details'>
            <h3>Order Details</h3>
            <p><strong>SubOrder Number:</strong> {{SubOrderNumber}}</p>
            <p><strong>Order Date:</strong> {{OrderDate}}</p>
            <p><strong>Amount:</strong> ${{Amount}}</p>
            <p><strong>Items:</strong> {{ItemCount}}</p>
        </div>
        <p>Please log in to your seller dashboard to process this order. Remember to ship within 3 business days.</p>
        <p>Best regards,<br>Mercato Team</p>
    </div>
    <div class='footer'>
        <p>&copy; 2024 Mercato. All rights reserved.</p>
    </div>
</body>
</html>";
    }

    private string GetOrderStatusChangedTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .header { background-color: #FF9800; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; }
        .footer { background-color: #f4f4f4; padding: 10px; text-align: center; font-size: 12px; }
        .status-box { background-color: #fff3cd; padding: 15px; margin: 15px 0; border-left: 4px solid #FF9800; }
    </style>
</head>
<body>
    <div class='header'>
        <h1>Order Status Update</h1>
    </div>
    <div class='content'>
        <p>Dear {{CustomerName}},</p>
        <p>Your order status has been updated.</p>
        <div class='status-box'>
            <p><strong>Order Number:</strong> {{OrderNumber}}</p>
            <p><strong>New Status:</strong> {{NewStatus}}</p>
            <p><strong>Tracking Number:</strong> {{TrackingNumber}}</p>
        </div>
        <p>You can view more details in your order history.</p>
        <p>Best regards,<br>Mercato Team</p>
    </div>
    <div class='footer'>
        <p>&copy; 2024 Mercato. All rights reserved.</p>
    </div>
</body>
</html>";
    }

    private string GetPaymentStatusChangedTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .header { background-color: #9C27B0; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; }
        .footer { background-color: #f4f4f4; padding: 10px; text-align: center; font-size: 12px; }
        .payment-box { background-color: #f3e5f5; padding: 15px; margin: 15px 0; border-left: 4px solid #9C27B0; }
    </style>
</head>
<body>
    <div class='header'>
        <h1>Payment Status Update</h1>
    </div>
    <div class='content'>
        <p>Dear {{CustomerName}},</p>
        <p>Your payment status has been updated.</p>
        <div class='payment-box'>
            <p><strong>Order Number:</strong> {{OrderNumber}}</p>
            <p><strong>Payment Status:</strong> {{PaymentStatus}}</p>
            <p><strong>Amount:</strong> ${{Amount}}</p>
        </div>
        <p>If you have any questions, please contact our support team.</p>
        <p>Best regards,<br>Mercato Team</p>
    </div>
    <div class='footer'>
        <p>&copy; 2024 Mercato. All rights reserved.</p>
    </div>
</body>
</html>";
    }

    private string GetCaseCreatedTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .header { background-color: #f44336; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; }
        .footer { background-color: #f4f4f4; padding: 10px; text-align: center; font-size: 12px; }
        .case-box { background-color: #ffebee; padding: 15px; margin: 15px 0; border-left: 4px solid #f44336; }
    </style>
</head>
<body>
    <div class='header'>
        <h1>New Case Created</h1>
    </div>
    <div class='content'>
        <p>Hello {{SellerName}},</p>
        <p>A customer has created a new {{CaseType}} case for your attention.</p>
        <div class='case-box'>
            <p><strong>Case Number:</strong> {{CaseNumber}}</p>
            <p><strong>Case Type:</strong> {{CaseType}}</p>
            <p><strong>Order Number:</strong> {{OrderNumber}}</p>
            <p><strong>Reason:</strong> {{Reason}}</p>
        </div>
        <p>Please review this case in your seller dashboard and respond promptly.</p>
        <p>Best regards,<br>Mercato Team</p>
    </div>
    <div class='footer'>
        <p>&copy; 2024 Mercato. All rights reserved.</p>
    </div>
</body>
</html>";
    }

    private string GetCaseMessageReceivedTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .header { background-color: #607D8B; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; }
        .footer { background-color: #f4f4f4; padding: 10px; text-align: center; font-size: 12px; }
        .message-box { background-color: #eceff1; padding: 15px; margin: 15px 0; border-left: 4px solid #607D8B; }
    </style>
</head>
<body>
    <div class='header'>
        <h1>New Message in Case</h1>
    </div>
    <div class='content'>
        <p>Hello {{RecipientName}},</p>
        <p>You have received a new message in case {{CaseNumber}}.</p>
        <div class='message-box'>
            <p><strong>From:</strong> {{SenderName}}</p>
            <p><strong>Message:</strong></p>
            <p>{{Message}}</p>
        </div>
        <p>Please log in to view the full conversation and respond.</p>
        <p>Best regards,<br>Mercato Team</p>
    </div>
    <div class='footer'>
        <p>&copy; 2024 Mercato. All rights reserved.</p>
    </div>
</body>
</html>";
    }

    private string GetDefaultTemplate()
    {
        return @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .header { background-color: #333; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; }
        .footer { background-color: #f4f4f4; padding: 10px; text-align: center; font-size: 12px; }
    </style>
</head>
<body>
    <div class='header'>
        <h1>Notification from Mercato</h1>
    </div>
    <div class='content'>
        <p>{{Message}}</p>
    </div>
    <div class='footer'>
        <p>&copy; 2024 Mercato. All rights reserved.</p>
    </div>
</body>
</html>";
    }
}
