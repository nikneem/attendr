using HexMaster.Attendr.Notifications.Abstractions.DomainModels;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Notifications.Services;

/// <summary>
/// Service for sending email notifications.
/// </summary>
public interface IEmailNotificationService
{
    /// <summary>
    /// Sends an email notification to a profile.
    /// </summary>
    Task SendEmailAsync(
        INotification notification,
        string recipientEmail,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Email notification service implementation.
/// This is a placeholder that logs email sends without actually sending them.
/// In production, this would integrate with an email service provider (SendGrid, AWS SES, etc.).
/// </summary>
public sealed class EmailNotificationService : IEmailNotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(ILogger<EmailNotificationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task SendEmailAsync(
        INotification notification,
        string recipientEmail,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            throw new ArgumentException("Recipient email cannot be null or empty", nameof(recipientEmail));
        }

        // TODO: Implement actual email sending logic
        // This would typically involve:
        // 1. Loading email template
        // 2. Replacing placeholders with notification data
        // 3. Calling email service provider API (SendGrid, AWS SES, etc.)
        // 4. Handling retries and failures
        // 5. Updating notification delivery status

        _logger.LogInformation(
            "EMAIL PLACEHOLDER: Would send email to {RecipientEmail} - Type: {TypeKey}, Title: {Title}, Message: {Message}",
            recipientEmail,
            notification.TypeKey,
            notification.Title,
            notification.Message);

        // Simulate async operation
        return Task.CompletedTask;
    }
}
