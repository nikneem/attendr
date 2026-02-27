using HexMaster.Attendr.Notifications.Abstractions.DTOs;
using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.Abstractions.Repositories;
using HexMaster.Attendr.Notifications.Abstractions.Services;
using HexMaster.Attendr.Notifications.DomainModels;
using HexMaster.Attendr.Notifications.Models;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HexMaster.Attendr.Notifications.Api.Endpoints;

/// <summary>
/// Endpoints for fetching and managing detailed notification preferences.
/// Combines user preferences with notification type configurations.
/// </summary>
public static class NotificationPreferencesDetailEndpoints
{
    public static IEndpointRouteBuilder MapNotificationPreferencesDetailEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications/preferences/detailed")
            .WithName("NotificationPreferencesDetail")
            .WithTags("Notification Preferences")
            .RequireAuthorization();

        group.MapGet("/", GetDetailedPreferences)
            .WithName("GetDetailedNotificationPreferences")
            .Produces<NotificationPreferencesDetailDto>(StatusCodes.Status200OK);

        group.MapPut("/", UpdateDetailedPreferences)
            .WithName("UpdateDetailedNotificationPreferences")
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    /// <summary>
    /// Gets detailed notification preferences for the current user,
    /// combining their saved preferences with notification type configuration.
    /// </summary>
    private static async Task<Ok<NotificationPreferencesDetailDto>> GetDetailedPreferences(
        IProfilesIntegrationService profilesIntegration,
        HttpContext httpContext,
        INotificationPreferencesRepository preferencesRepository,
        INotificationTypeService typeService)
    {
        var resolvedProfile = await profilesIntegration.GetProfileFromUser(httpContext.User, httpContext.RequestAborted);
        var profileId = Guid.Parse(resolvedProfile.ProfileId);

        // Get user's saved preferences
        var userPreferences = await preferencesRepository.GetByProfileIdAsync(profileId);

        // Get all notification types
        var allTypes = typeService.GetAllTypes();

        // Build detailed preferences
        var notificationTypePrefs = allTypes
            .Cast<Models.NotificationType>()
            .Select(type => new NotificationTypePreferenceDto
            {
                TypeKey = type.TypeKey,
                DisplayName = type.DisplayName,
                Description = type.Description,
                ChannelPreferences = BuildChannelPreferences(
                    type,
                    userPreferences?.TypeChannelPreferences?.TryGetValue(type.TypeKey, out var prefs) == true
                        ? prefs
                        : null)
            })
            .ToList();

        var result = new NotificationPreferencesDetailDto
        {
            ProfileId = profileId,
            UpdatedAt = userPreferences?.CreatedAt,
            DoNotDisturbUntil = userPreferences?.DoNotDisturbUntil,
            NotificationTypes = notificationTypePrefs
        };

        return TypedResults.Ok(result);
    }

    /// <summary>
    /// Updates detailed notification preferences for the current user.
    /// </summary>
    private static async Task<NoContent> UpdateDetailedPreferences(
        IProfilesIntegrationService profilesIntegration,
        HttpContext httpContext,
        INotificationPreferencesRepository preferencesRepository,
        INotificationTypeService typeService,
        [FromBody] UpdateDetailedPreferencesRequest request)
    {
        var resolvedProfile = await profilesIntegration.GetProfileFromUser(httpContext.User, httpContext.RequestAborted);
        var profileId = Guid.Parse(resolvedProfile.ProfileId);

        // Build TypeChannelPreferences dictionary from the request
        var typeChannelPrefs = new Dictionary<string, Dictionary<NotificationChannel, bool>>();

        foreach (var typePrefs in request.NotificationTypes)
        {
            var channelPrefs = new Dictionary<NotificationChannel, bool>();

            foreach (var channelPrefs_kvp in typePrefs.ChannelPreferences)
            {
                if (Enum.TryParse<NotificationChannel>(channelPrefs_kvp.Key, out var channel))
                {
                    channelPrefs[channel] = channelPrefs_kvp.Value;
                }
            }

            typeChannelPrefs[typePrefs.TypeKey] = channelPrefs;
        }

        var preferences = new NotificationPreferences
        {
            ProfileId = profileId,
            TypeChannelPreferences = typeChannelPrefs,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await preferencesRepository.UpsertAsync(preferences);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Builds channel preferences for a notification type,
    /// combining available channels, defaults, and user preferences.
    /// </summary>
    private static Dictionary<string, ChannelPreferenceDto> BuildChannelPreferences(
        Models.NotificationType type,
        Dictionary<NotificationChannel, bool>? userPrefs)
    {
        var channelPrefs = new Dictionary<string, ChannelPreferenceDto>();

        foreach (var channel in Enum.GetValues<NotificationChannel>())
        {
            var isAvailable = type.AvailableChannels.TryGetValue(channel, out var available) && available;
            var isDefaultEnabled = type.DefaultChannelSettings.TryGetValue(channel, out var defaultEnabled) && defaultEnabled;
            var isEnabled = userPrefs?.TryGetValue(channel, out var userEnabled) == true ? userEnabled : isDefaultEnabled;

            channelPrefs[channel.ToString()] = new ChannelPreferenceDto
            {
                ChannelName = channel.ToString(),
                IsAvailable = isAvailable,
                IsEnabled = isEnabled && isAvailable, // Can only be enabled if available
                IsDefaultEnabled = isDefaultEnabled
            };
        }

        return channelPrefs;
    }
}

/// <summary>
/// Request to update detailed notification preferences.
/// </summary>
public sealed class UpdateDetailedPreferencesRequest
{
    /// <summary>
    /// The notification type preferences to update.
    /// </summary>
    public required List<UpdateNotificationTypePreferenceRequest> NotificationTypes { get; init; }
}

/// <summary>
/// Request to update preferences for a specific notification type.
/// </summary>
public sealed class UpdateNotificationTypePreferenceRequest
{
    /// <summary>
    /// The notification type key.
    /// </summary>
    public required string TypeKey { get; init; }

    /// <summary>
    /// Channel preferences for this notification type.
    /// Key is the channel name (InApp, Email, Push), value is whether it's enabled.
    /// </summary>
    public required Dictionary<string, bool> ChannelPreferences { get; init; }
}
