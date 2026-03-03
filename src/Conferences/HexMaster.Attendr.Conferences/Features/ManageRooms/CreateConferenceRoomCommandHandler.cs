using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Core.CommandHandlers;

using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.ManageRooms;

public sealed class CreateConferenceRoomCommandHandler : ICommandHandler<CreateConferenceRoomCommand, ConferenceRoomDto>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<CreateConferenceRoomCommandHandler> _logger;

    public CreateConferenceRoomCommandHandler(IConferenceRepository repository, ILogger<CreateConferenceRoomCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConferenceRoomDto> Handle(CreateConferenceRoomCommand command, CancellationToken cancellationToken = default)
    {
        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference {command.ConferenceId} not found.");

        var room = Room.Create(command.Name, command.Capacity);
        conference.AddRoom(room);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Created room {RoomId} for conference {ConferenceId}", room.Id, command.ConferenceId);
        return new ConferenceRoomDto(room.Id, room.Name, room.Capacity);
    }
}
