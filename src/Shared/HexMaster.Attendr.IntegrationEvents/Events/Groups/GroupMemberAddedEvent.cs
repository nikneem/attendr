using HexMaster.Attendr.IntegrationEvents.Constants;

namespace HexMaster.Attendr.IntegrationEvents.Events.Groups;

/// <summary>
/// Integration event published when a member is added to a group.
/// This can occur through approving a join request or direct invitation.
/// </summary>
public sealed class GroupMemberAddedEvent : IntegrationEvent
{
    public override string EventType => IntegrationEventTopics.GroupMemberAdded;

    public Guid GroupId { get; init; }
    public string GroupName { get; init; } = string.Empty;
    public Guid ProfileId { get; init; }
    public string Role { get; init; } = string.Empty;
}
