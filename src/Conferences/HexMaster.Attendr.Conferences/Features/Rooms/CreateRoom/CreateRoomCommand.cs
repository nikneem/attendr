using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.Rooms.CreateRoom;

public sealed record CreateRoomCommand(
    Guid ConferenceId,
    string Name,
    int Capacity,
    Guid? RequestingProfileId,
    bool IsAdmin) : IAttendrCommand;
