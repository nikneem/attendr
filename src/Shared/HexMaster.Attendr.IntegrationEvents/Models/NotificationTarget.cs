namespace HexMaster.Attendr.IntegrationEvents.Models;

/// <summary>
/// Represents a target recipient for a notification.
/// Used in integration events to specify which users should receive notifications.
/// </summary>
public sealed class NotificationTarget
{
    /// <summary>
    /// The unique identifier of the profile to notify.
    /// </summary>
    public required Guid ProfileId { get; init; }

    /// <summary>
    /// The display name of the profile.
    /// </summary>
    public required string ProfileName { get; init; }
}
