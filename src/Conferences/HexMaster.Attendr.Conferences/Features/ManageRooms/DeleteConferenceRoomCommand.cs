using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.Features.ManageRooms;

public sealed record DeleteConferenceRoomCommand(Guid ConferenceId, Guid RoomId) : IAttendrCommand;
