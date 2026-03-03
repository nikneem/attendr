using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.Rooms.UpdateRoom;

public sealed class UpdateRoomCommandHandler : ICommandHandler<UpdateRoomCommand, ConferenceRoomDto>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<UpdateRoomCommandHandler> _logger;

    public UpdateRoomCommandHandler(IConferenceRepository repository, ILogger<UpdateRoomCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConferenceRoomDto> Handle(UpdateRoomCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        using var activity = ActivitySources.Conferences.StartActivity("UpdateRoom", ActivityKind.Internal);
        activity?.SetTag("conference.id", command.ConferenceId);
        activity?.SetTag("room.id", command.RoomId);

        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference with ID {command.ConferenceId} not found");

        if (!command.IsAdmin && conference.CreatedByProfileId != command.RequestingProfileId)
            throw new ForbiddenException("You are not authorized to modify this conference.", "https://attendr.dev/errors/forbidden");

        var room = conference.Rooms.FirstOrDefault(r => r.Id == command.RoomId)
            ?? throw new KeyNotFoundException($"Room with ID {command.RoomId} not found");

        room.SetName(command.Name);
        room.SetCapacity(command.Capacity);
        conference.UpdateRoom(room);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Room {RoomId} updated in conference {ConferenceId}", command.RoomId, command.ConferenceId);

        return new ConferenceRoomDto(room.Id, room.Name, room.Capacity, room.ExternalId);
    }
}
