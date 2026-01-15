using HexMaster.Attendr.IntegrationEvents.Events;

namespace HexMaster.Attendr.Notifications.Abstractions.IntegrationEvents;

/// <summary>
/// Base event for all notification-triggering events.
/// Other services can publish events that the Notifications service will consume.
/// </summary>
public abstract class NotificationTriggerEvent : IntegrationEvent
{
    /// <summary>
    /// The profile ID to send the notification to.
    /// </summary>
    public required Guid ProfileId { get; init; }

    /// <summary>
    /// The notification type key.
    /// </summary>
    public required string NotificationTypeKey { get; init; }

    /// <summary>
    /// The notification title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// The notification message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Optional URL to link to.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// Optional actor who triggered this.
    /// </summary>
    public Guid? ActorId { get; init; }

    /// <summary>
    /// Optional entity references.
    /// </summary>
    public Dictionary<string, string>? EntityRefs { get; init; }

    /// <summary>
    /// Optional stack key for grouping notifications.
    /// </summary>
    public string? StackKey { get; init; }
}
