using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.Speakers.UpdateSpeaker;

public sealed class UpdateSpeakerCommandHandler : ICommandHandler<UpdateSpeakerCommand, ConferenceSpeakerDto>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<UpdateSpeakerCommandHandler> _logger;

    public UpdateSpeakerCommandHandler(IConferenceRepository repository, ILogger<UpdateSpeakerCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConferenceSpeakerDto> Handle(UpdateSpeakerCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        using var activity = ActivitySources.Conferences.StartActivity("UpdateSpeaker", ActivityKind.Internal);
        activity?.SetTag("conference.id", command.ConferenceId);
        activity?.SetTag("speaker.id", command.SpeakerId);

        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference with ID {command.ConferenceId} not found");

        if (!command.IsAdmin && conference.CreatedByProfileId != command.RequestingProfileId)
            throw new ForbiddenException("You are not authorized to modify this conference.", "https://attendr.dev/errors/forbidden");

        var speaker = conference.Speakers.FirstOrDefault(s => s.Id == command.SpeakerId)
            ?? throw new KeyNotFoundException($"Speaker with ID {command.SpeakerId} not found");

        speaker.SetName(command.Name);
        speaker.SetCompany(command.Company);
        speaker.SetProfilePictureUrl(command.ProfilePictureUrl);
        conference.UpdateSpeaker(speaker);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Speaker {SpeakerId} updated in conference {ConferenceId}", command.SpeakerId, command.ConferenceId);

        return new ConferenceSpeakerDto(speaker.Id, speaker.Name, speaker.Company, speaker.ProfilePictureUrl, speaker.ExternalId);
    }
}
