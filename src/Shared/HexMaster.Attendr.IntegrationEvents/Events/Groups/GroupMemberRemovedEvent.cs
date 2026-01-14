using HexMaster.Attendr.IntegrationEvents.Constants;

namespace HexMaster.Attendr.IntegrationEvents.Events.Groups;

/// <summary>
/// Integration event published when a member is removed from a group.
/// </summary>
public sealed class GroupMemberRemovedEvent : IntegrationEvent
{
    public override string EventType => IntegrationEventTopics.GroupMemberRemoved;

    public Guid GroupId { get; init; }
    public string GroupName { get; init; } = string.Empty;
    public Guid ProfileId { get; init; }
}
