using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;

using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.ManageRooms;

public sealed class UpdateConferenceRoomCommandHandler : ICommandHandler<UpdateConferenceRoomCommand, ConferenceRoomDto>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<UpdateConferenceRoomCommandHandler> _logger;

    public UpdateConferenceRoomCommandHandler(IConferenceRepository repository, ILogger<UpdateConferenceRoomCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConferenceRoomDto> Handle(UpdateConferenceRoomCommand command, CancellationToken cancellationToken = default)
    {
        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference {command.ConferenceId} not found.");

        var room = conference.Rooms.FirstOrDefault(r => r.Id == command.RoomId)
            ?? throw new KeyNotFoundException($"Room {command.RoomId} not found in conference {command.ConferenceId}.");

        room.SetName(command.Name);
        room.SetCapacity(command.Capacity);
        conference.UpdateRoom(room);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Updated room {RoomId} for conference {ConferenceId}", command.RoomId, command.ConferenceId);
        return new ConferenceRoomDto(room.Id, room.Name, room.Capacity);
    }
}
