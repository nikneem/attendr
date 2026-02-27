using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.Rooms.DeleteRoom;

public sealed class DeleteRoomCommandHandler : ICommandHandler<DeleteRoomCommand, bool>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<DeleteRoomCommandHandler> _logger;

    public DeleteRoomCommandHandler(IConferenceRepository repository, ILogger<DeleteRoomCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteRoomCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        using var activity = ActivitySources.Conferences.StartActivity("DeleteRoom", ActivityKind.Internal);
        activity?.SetTag("conference.id", command.ConferenceId);
        activity?.SetTag("room.id", command.RoomId);

        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference with ID {command.ConferenceId} not found");

        if (!command.IsAdmin && conference.CreatedByProfileId != command.RequestingProfileId)
            throw new ForbiddenException("You are not authorized to modify this conference.", "https://attendr.dev/errors/forbidden");

        if (!conference.Rooms.Any(r => r.Id == command.RoomId))
            return false;

        conference.RemoveRoom(command.RoomId);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Room {RoomId} deleted from conference {ConferenceId}", command.RoomId, command.ConferenceId);

        return true;
    }
}
