using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.Abstractions.Services;
using HexMaster.Attendr.Notifications.Services;
using HexMaster.Attendr.Profiles.Integrations.Services;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Notifications.Features.ProcessNotificationTrigger;

/// <summary>
/// Command to process a notification trigger event.
/// </summary>
public sealed record ProcessNotificationTriggerCommand(
    Guid ProfileId,
    string TypeKey,
    string Title,
    string Message,
    string? Url = null,
    Guid? ActorId = null,
    Dictionary<string, string>? EntityRefs = null,
    string? StackKey = null);

/// <summary>
/// Handler for processing notification trigger commands.
/// Checks notification preferences and routes to appropriate channels (InApp, Email, Push).
/// </summary>
public sealed class ProcessNotificationTriggerCommandHandler(
    INotificationService notificationService,
    INotificationTypeService notificationTypeService,
    INotificationPreferencesCacheService preferencesCacheService,
    IProfilesIntegrationService profilesIntegrationService,
    IEmailNotificationService emailService,
    IPushNotificationService pushService,
    ILogger<ProcessNotificationTriggerCommandHandler> logger)
    : ICommandHandler<ProcessNotificationTriggerCommand>
{
    private readonly INotificationService _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    private readonly INotificationTypeService _notificationTypeService = notificationTypeService ?? throw new ArgumentNullException(nameof(notificationTypeService));
    private readonly INotificationPreferencesCacheService _preferencesCacheService = preferencesCacheService ?? throw new ArgumentNullException(nameof(preferencesCacheService));
    private readonly IProfilesIntegrationService _profilesIntegrationService = profilesIntegrationService ?? throw new ArgumentNullException(nameof(profilesIntegrationService));
    private readonly IEmailNotificationService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    private readonly IPushNotificationService _pushService = pushService ?? throw new ArgumentNullException(nameof(pushService));
    private readonly ILogger<ProcessNotificationTriggerCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task Handle(ProcessNotificationTriggerCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing notification trigger for profile {ProfileId} of type {TypeKey}",
                command.ProfileId, command.TypeKey);

            // Get notification type configuration
            var notificationType = _notificationTypeService.GetTypeByKey(command.TypeKey);
            if (notificationType == null)
            {
                _logger.LogWarning("Unknown notification type: {TypeKey}", command.TypeKey);
                return;
            }

            // Get user preferences from cache (or repository if not cached)
            var preferences = await _preferencesCacheService.GetOrFetchPreferencesAsync(
                command.ProfileId,
                cancellationToken);

            // Check if in DND mode
            if (preferences?.DoNotDisturbUntil.HasValue == true && 
                preferences.DoNotDisturbUntil.Value > DateTime.UtcNow)
            {
                _logger.LogInformation(
                    "Profile {ProfileId} is in Do Not Disturb mode until {Until}, skipping all notification channels",
                    command.ProfileId,
                    preferences.DoNotDisturbUntil.Value);
                return;
            }

            // Always create in-app notification
            var notification = await _notificationService.CreateNotificationAsync(
                command.ProfileId,
                command.TypeKey,
                command.Title,
                command.Message,
                command.Url,
                command.ActorId,
                command.EntityRefs,
                command.StackKey,
                cancellationToken);

            _logger.LogInformation(
                "Created in-app notification {NotificationId} for profile {ProfileId}",
                notification.Id,
                command.ProfileId);

            // Check and send to Email channel if enabled
            if (IsChannelEnabled(notificationType, preferences, NotificationChannel.Email, command.TypeKey))
            {
                await SendEmailNotificationAsync(command.ProfileId, notification, cancellationToken);
            }
            else
            {
                _logger.LogDebug(
                    "Email channel not enabled for profile {ProfileId}, type {TypeKey}",
                    command.ProfileId,
                    command.TypeKey);
            }

            // Check and send to Push channel if enabled
            if (IsChannelEnabled(notificationType, preferences, NotificationChannel.Push, command.TypeKey))
            {
                await SendPushNotificationAsync(command.ProfileId, command.Title, command.Message, command.Url, cancellationToken);
            }
            else
            {
                _logger.LogDebug(
                    "Push channel not enabled for profile {ProfileId}, type {TypeKey}",
                    command.ProfileId,
                    command.TypeKey);
            }

            _logger.LogInformation(
                "Successfully processed notification trigger for profile {ProfileId}",
                command.ProfileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process notification trigger for profile {ProfileId} of type {TypeKey}",
                command.ProfileId, command.TypeKey);
            throw;
        }
    }

    /// <summary>
    /// Determines if a specific notification channel is enabled for a notification type.
    /// Checks both the notification type configuration and user preferences.
    /// </summary>
    private bool IsChannelEnabled(
        Abstractions.Models.INotificationType notificationType,
        Abstractions.DomainModels.INotificationPreferences? preferences,
        NotificationChannel channel,
        string typeKey)
    {
        // Check if channel is available for this notification type
        if (!notificationType.AvailableChannels.TryGetValue(channel, out var isAvailable) || !isAvailable)
        {
            return false;
        }

        // Get user's preference for this type and channel
        if (preferences?.TypeChannelPreferences.TryGetValue(typeKey, out var channelPrefs) == true)
        {
            if (channelPrefs.TryGetValue(channel, out var isEnabled))
            {
                return isEnabled;
            }
        }

        // Fall back to default setting from notification type
        return notificationType.DefaultChannelSettings.TryGetValue(channel, out var defaultEnabled) && defaultEnabled;
    }

    /// <summary>
    /// Sends an email notification to a profile.
    /// Fetches the profile's email address and sends the notification.
    /// </summary>
    private async Task SendEmailNotificationAsync(
        Guid profileId,
        Abstractions.DomainModels.INotification notification,
        CancellationToken cancellationToken)
    {
        try
        {
            // Fetch profile details to get email address
            var profileDetails = await _profilesIntegrationService.GetProfileDetails(
                profileId.ToString(),
                cancellationToken);

            if (profileDetails == null)
            {
                _logger.LogWarning(
                    "Could not fetch profile details for {ProfileId}, skipping email notification",
                    profileId);
                return;
            }

            await _emailService.SendEmailAsync(notification, profileDetails.Email, cancellationToken);

            _logger.LogInformation(
                "Sent email notification to {Email} for notification {NotificationId}",
                profileDetails.Email,
                notification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send email notification for profile {ProfileId}, notification {NotificationId}",
                profileId,
                notification.Id);
            // Don't rethrow - email failure shouldn't fail the entire notification
        }
    }

    /// <summary>
    /// Sends a push notification to all subscribed devices for a profile.
    /// Uses the existing PushNotificationService.SendAsync method.
    /// </summary>
    private async Task SendPushNotificationAsync(
        Guid profileId,
        string title,
        string message,
        string? url,
        CancellationToken cancellationToken)
    {
        try
        {
            var sentCount = await _pushService.SendAsync(profileId, title, message, url, cancellationToken);

            _logger.LogInformation(
                "Sent push notification to {SentCount} subscription(s) for profile {ProfileId}",
                sentCount,
                profileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send push notification for profile {ProfileId}",
                profileId);
            // Don't rethrow - push failure shouldn't fail the entire notification
        }
    }
}
