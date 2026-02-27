using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.Rooms.UpdateRoom;

public sealed record UpdateRoomCommand(
    Guid ConferenceId,
    Guid RoomId,
    string Name,
    int Capacity,
    Guid? RequestingProfileId,
    bool IsAdmin) : IAttendrCommand;
