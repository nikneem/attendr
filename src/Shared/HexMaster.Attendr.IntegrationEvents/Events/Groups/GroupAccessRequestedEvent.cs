using HexMaster.Attendr.IntegrationEvents.Constants;
using HexMaster.Attendr.IntegrationEvents.Models;

namespace HexMaster.Attendr.IntegrationEvents.Events.Groups;

/// <summary>
/// Integration event published when a user requests access to a private group.
/// This event triggers notifications to group owners/administrators to review and approve the request.
/// </summary>
public sealed class GroupAccessRequestedEvent : IntegrationEvent
{
    public override string EventType => IntegrationEventTopics.GroupAccessRequested;

    /// <summary>
    /// The unique identifier of the group.
    /// </summary>
    public required Guid GroupId { get; init; }

    /// <summary>
    /// The name of the group.
    /// </summary>
    public required string GroupName { get; init; }

    /// <summary>
    /// The unique identifier of the profile requesting access.
    /// </summary>
    public required Guid ProfileId { get; init; }

    /// <summary>
    /// The display name of the profile requesting access.
    /// </summary>
    public required string ProfileName { get; init; }

    /// <summary>
    /// The timestamp when the access request was created.
    /// </summary>
    public required DateTimeOffset CreatedOn { get; init; }

    /// <summary>
    /// The list of profiles who should be notified about this access request.
    /// Typically includes group owners and administrators.
    /// </summary>
    public required List<NotificationTarget> NotificationTargets { get; init; }
}
