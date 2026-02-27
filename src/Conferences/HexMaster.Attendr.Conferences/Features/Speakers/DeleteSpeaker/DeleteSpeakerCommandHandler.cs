using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.Speakers.DeleteSpeaker;

public sealed class DeleteSpeakerCommandHandler : ICommandHandler<DeleteSpeakerCommand, bool>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<DeleteSpeakerCommandHandler> _logger;

    public DeleteSpeakerCommandHandler(IConferenceRepository repository, ILogger<DeleteSpeakerCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteSpeakerCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        using var activity = ActivitySources.Conferences.StartActivity("DeleteSpeaker", ActivityKind.Internal);
        activity?.SetTag("conference.id", command.ConferenceId);
        activity?.SetTag("speaker.id", command.SpeakerId);

        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference with ID {command.ConferenceId} not found");

        if (!command.IsAdmin && conference.CreatedByProfileId != command.RequestingProfileId)
            throw new ForbiddenException("You are not authorized to modify this conference.", "https://attendr.dev/errors/forbidden");

        if (!conference.Speakers.Any(s => s.Id == command.SpeakerId))
            return false;

        conference.RemoveSpeaker(command.SpeakerId);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Speaker {SpeakerId} deleted from conference {ConferenceId}", command.SpeakerId, command.ConferenceId);

        return true;
    }
}
