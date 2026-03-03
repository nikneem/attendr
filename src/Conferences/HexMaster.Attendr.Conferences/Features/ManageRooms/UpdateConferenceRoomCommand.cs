using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.ManageRooms;

public sealed record UpdateConferenceRoomCommand(Guid ConferenceId, Guid RoomId, string Name, int Capacity) : IAttendrCommand;
