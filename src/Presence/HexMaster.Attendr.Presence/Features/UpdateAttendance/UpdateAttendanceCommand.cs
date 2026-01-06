
using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Presence.Features.UpdateAttendance;

public sealed record UpdateAttendanceCommand(Guid ConferenceId, Guid ProfileId, bool IsAttending) : IAttendrCommand;
