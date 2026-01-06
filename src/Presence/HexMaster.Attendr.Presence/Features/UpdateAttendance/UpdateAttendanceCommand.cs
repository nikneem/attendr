using HexMaster.Attendr.Core.Commands;

namespace HexMaster.Attendr.Presence.Features.UpdateAttendance;

public sealed record UpdateAttendanceCommand(Guid ConferenceId, Guid ProfileId, bool IsAttending) : ICommand;
