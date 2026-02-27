using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.Speakers.CreateSpeaker;

public sealed class CreateSpeakerCommandHandler : ICommandHandler<CreateSpeakerCommand, ConferenceSpeakerDto>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<CreateSpeakerCommandHandler> _logger;

    public CreateSpeakerCommandHandler(IConferenceRepository repository, ILogger<CreateSpeakerCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConferenceSpeakerDto> Handle(CreateSpeakerCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        using var activity = ActivitySources.Conferences.StartActivity("CreateSpeaker", ActivityKind.Internal);
        activity?.SetTag("conference.id", command.ConferenceId);

        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference with ID {command.ConferenceId} not found");

        if (!command.IsAdmin && conference.CreatedByProfileId != command.RequestingProfileId)
            throw new ForbiddenException("You are not authorized to modify this conference.", "https://attendr.dev/errors/forbidden");

        var speaker = Speaker.Create(command.Name, command.Company, command.ProfilePictureUrl);
        conference.AddSpeaker(speaker);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Speaker {SpeakerId} created for conference {ConferenceId}", speaker.Id, command.ConferenceId);

        return new ConferenceSpeakerDto(speaker.Id, speaker.Name, speaker.Company, speaker.ProfilePictureUrl, speaker.ExternalId);
    }
}
