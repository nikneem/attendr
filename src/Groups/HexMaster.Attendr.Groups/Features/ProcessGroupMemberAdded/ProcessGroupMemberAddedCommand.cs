using HexMaster.Attendr.IntegrationEvents.Events.Groups;

namespace HexMaster.Attendr.Groups.Features.ProcessGroupMemberAdded;

/// <summary>
/// Command to process a GroupMemberAdded integration event.
/// </summary>
public sealed record ProcessGroupMemberAddedCommand(GroupMemberAddedEvent Event);
