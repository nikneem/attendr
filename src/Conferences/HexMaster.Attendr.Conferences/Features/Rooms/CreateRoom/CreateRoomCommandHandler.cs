using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.Rooms.CreateRoom;

public sealed class CreateRoomCommandHandler : ICommandHandler<CreateRoomCommand, ConferenceRoomDto>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<CreateRoomCommandHandler> _logger;

    public CreateRoomCommandHandler(IConferenceRepository repository, ILogger<CreateRoomCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConferenceRoomDto> Handle(CreateRoomCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        using var activity = ActivitySources.Conferences.StartActivity("CreateRoom", ActivityKind.Internal);
        activity?.SetTag("conference.id", command.ConferenceId);

        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference with ID {command.ConferenceId} not found");

        if (!command.IsAdmin && conference.CreatedByProfileId != command.RequestingProfileId)
            throw new ForbiddenException("You are not authorized to modify this conference.", "https://attendr.dev/errors/forbidden");

        var room = Room.Create(command.Name, command.Capacity);
        conference.AddRoom(room);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Room {RoomId} created for conference {ConferenceId}", room.Id, command.ConferenceId);

        return new ConferenceRoomDto(room.Id, room.Name, room.Capacity, room.ExternalId);
    }
}
