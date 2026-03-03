using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;

using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.ManageSpeakers;

public sealed class UpdateConferenceSpeakerCommandHandler : ICommandHandler<UpdateConferenceSpeakerCommand, ConferenceSpeakerDto>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<UpdateConferenceSpeakerCommandHandler> _logger;

    public UpdateConferenceSpeakerCommandHandler(IConferenceRepository repository, ILogger<UpdateConferenceSpeakerCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConferenceSpeakerDto> Handle(UpdateConferenceSpeakerCommand command, CancellationToken cancellationToken = default)
    {
        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference {command.ConferenceId} not found.");

        var speaker = conference.Speakers.FirstOrDefault(s => s.Id == command.SpeakerId)
            ?? throw new KeyNotFoundException($"Speaker {command.SpeakerId} not found in conference {command.ConferenceId}.");

        speaker.SetName(command.Name);
        speaker.SetCompany(command.Company);
        speaker.SetProfilePictureUrl(command.ProfilePictureUrl);

        conference.UpdateSpeaker(speaker);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Updated speaker {SpeakerId} for conference {ConferenceId}", command.SpeakerId, command.ConferenceId);
        return new ConferenceSpeakerDto(speaker.Id, speaker.Name, speaker.Company, speaker.ProfilePictureUrl);
    }
}
