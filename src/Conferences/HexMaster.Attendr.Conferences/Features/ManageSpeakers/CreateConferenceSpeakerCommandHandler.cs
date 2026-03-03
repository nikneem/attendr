using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Core.CommandHandlers;

using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.ManageSpeakers;

public sealed class CreateConferenceSpeakerCommandHandler : ICommandHandler<CreateConferenceSpeakerCommand, ConferenceSpeakerDto>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<CreateConferenceSpeakerCommandHandler> _logger;

    public CreateConferenceSpeakerCommandHandler(IConferenceRepository repository, ILogger<CreateConferenceSpeakerCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConferenceSpeakerDto> Handle(CreateConferenceSpeakerCommand command, CancellationToken cancellationToken = default)
    {
        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference {command.ConferenceId} not found.");

        var speaker = Speaker.Create(command.Name, command.Company, command.ProfilePictureUrl);
        conference.AddSpeaker(speaker);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Created speaker {SpeakerId} for conference {ConferenceId}", speaker.Id, command.ConferenceId);
        return new ConferenceSpeakerDto(speaker.Id, speaker.Name, speaker.Company, speaker.ProfilePictureUrl);
    }
}
