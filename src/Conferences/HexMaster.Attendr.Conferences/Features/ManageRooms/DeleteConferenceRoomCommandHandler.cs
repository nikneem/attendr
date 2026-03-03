using HexMaster.Attendr.Core.CommandHandlers;

using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.ManageRooms;

public sealed class DeleteConferenceRoomCommandHandler : ICommandHandler<DeleteConferenceRoomCommand, bool>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<DeleteConferenceRoomCommandHandler> _logger;

    public DeleteConferenceRoomCommandHandler(IConferenceRepository repository, ILogger<DeleteConferenceRoomCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteConferenceRoomCommand command, CancellationToken cancellationToken = default)
    {
        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference {command.ConferenceId} not found.");

        conference.RemoveRoom(command.RoomId);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Deleted room {RoomId} from conference {ConferenceId}", command.RoomId, command.ConferenceId);
        return true;
    }
}
