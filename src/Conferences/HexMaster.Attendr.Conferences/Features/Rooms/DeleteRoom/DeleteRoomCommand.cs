using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.Rooms.DeleteRoom;

public sealed record DeleteRoomCommand(
    Guid ConferenceId,
    Guid RoomId,
    Guid? RequestingProfileId,
    bool IsAdmin) : IAttendrCommand;
