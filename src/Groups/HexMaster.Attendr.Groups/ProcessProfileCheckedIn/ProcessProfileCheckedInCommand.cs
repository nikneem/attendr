using HexMaster.Attendr.IntegrationEvents.Events;

namespace HexMaster.Attendr.Groups.ProcessProfileCheckedIn;

/// <summary>
/// Command to process a ProfileCheckedIn event and update group activities.
/// </summary>
/// <param name="Event">The ProfileCheckedIn integration event.</param>
public sealed record ProcessProfileCheckedInCommand(ProfileCheckedInEvent Event);
