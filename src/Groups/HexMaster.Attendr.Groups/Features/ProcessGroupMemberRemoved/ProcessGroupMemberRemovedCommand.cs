using HexMaster.Attendr.IntegrationEvents.Events.Groups;

namespace HexMaster.Attendr.Groups.Features.ProcessGroupMemberRemoved;

/// <summary>
/// Command to process a GroupMemberRemoved integration event.
/// </summary>
public sealed record ProcessGroupMemberRemovedCommand(GroupMemberRemovedEvent Event);
