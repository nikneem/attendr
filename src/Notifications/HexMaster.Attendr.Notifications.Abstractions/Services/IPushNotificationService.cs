namespace HexMaster.Attendr.Notifications.Abstractions.Services;

/// <summary>
/// Service for sending push notifications to subscribed browsers.
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Sends a push notification to all subscriptions for a profile.
    /// </summary>
    /// <param name="profileId">The profile ID to send to</param>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message</param>
    /// <param name="url">Optional URL to navigate to when clicked</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of successful sends</returns>
    Task<int> SendAsync(
        Guid profileId,
        string title,
        string message,
        string? url = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a push notification to a specific subscription.
    /// </summary>
    /// <param name="endpoint">The subscription endpoint URL</param>
    /// <param name="p256dh">The P256DH key from the subscription</param>
    /// <param name="auth">The Auth key from the subscription</param>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message</param>
    /// <param name="url">Optional URL to navigate to when clicked</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendToSubscriptionAsync(
        string endpoint,
        string p256dh,
        string auth,
        string title,
        string message,
        string? url = null,
        CancellationToken cancellationToken = default);
}
