using HexMaster.Attendr.Core.CommandHandlers;

using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.ManageSpeakers;

public sealed class DeleteConferenceSpeakerCommandHandler : ICommandHandler<DeleteConferenceSpeakerCommand, bool>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<DeleteConferenceSpeakerCommandHandler> _logger;

    public DeleteConferenceSpeakerCommandHandler(IConferenceRepository repository, ILogger<DeleteConferenceSpeakerCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteConferenceSpeakerCommand command, CancellationToken cancellationToken = default)
    {
        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference {command.ConferenceId} not found.");

        conference.RemoveSpeaker(command.SpeakerId);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Deleted speaker {SpeakerId} from conference {ConferenceId}", command.SpeakerId, command.ConferenceId);
        return true;
    }
}
