using HexMaster.Attendr.IntegrationEvents.Events;

namespace HexMaster.Attendr.Groups.ProcessProfileConferenceAttendanceChanged;

/// <summary>
/// Command to process a ProfileConferenceAttendanceChanged event and update group activities.
/// </summary>
/// <param name="Event">The ProfileConferenceAttendanceChanged integration event.</param>
public sealed record ProcessProfileConferenceAttendanceChangedCommand(ProfileConferenceAttendanceChangedEvent Event);
