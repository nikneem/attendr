using HexMaster.Attendr.IntegrationEvents.Events.Profiles;

namespace HexMaster.Attendr.Groups.Features.ProcessProfileCheckedIn;

/// <summary>
/// Command to process a ProfileCheckedIn event and update group activities.
/// </summary>
/// <param name="Event">The ProfileCheckedIn integration event.</param>
public sealed record ProcessProfileCheckedInCommand(ProfileCheckedInEvent Event);
