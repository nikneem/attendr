using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.Presentations.UpdatePresentation;

public sealed class UpdatePresentationCommandHandler : ICommandHandler<UpdatePresentationCommand, ConferencePresentationDto>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<UpdatePresentationCommandHandler> _logger;

    public UpdatePresentationCommandHandler(IConferenceRepository repository, ILogger<UpdatePresentationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConferencePresentationDto> Handle(UpdatePresentationCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        using var activity = ActivitySources.Conferences.StartActivity("UpdatePresentation", ActivityKind.Internal);
        activity?.SetTag("conference.id", command.ConferenceId);
        activity?.SetTag("presentation.id", command.PresentationId);

        if (command.SpeakerIds == null || command.SpeakerIds.Count == 0)
            throw new ArgumentException("At least one speaker is required.", nameof(command.SpeakerIds));

        if (command.EndDateTime <= command.StartDateTime)
            throw new ArgumentException("End date/time must be after start date/time.", nameof(command.EndDateTime));

        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference with ID {command.ConferenceId} not found");

        if (!command.IsAdmin && conference.CreatedByProfileId != command.RequestingProfileId)
            throw new ForbiddenException("You are not authorized to modify this conference.", "https://attendr.dev/errors/forbidden");

        var presentation = conference.Presentations.FirstOrDefault(p => p.Id == command.PresentationId)
            ?? throw new KeyNotFoundException($"Presentation with ID {command.PresentationId} not found");

        var room = conference.Rooms.FirstOrDefault(r => r.Id == command.RoomId)
            ?? throw new KeyNotFoundException($"Room with ID {command.RoomId} not found in conference");

        presentation.UpdateDetails(command.Title, command.Abstract, command.StartDateTime, command.EndDateTime);
        presentation.ChangeRoom(room);

        // Sync speakers: add new ones first, then remove ones no longer needed
        var newSpeakerIds = command.SpeakerIds.ToHashSet();
        var currentSpeakerIds = presentation.Speakers.Select(s => s.Id).ToHashSet();

        foreach (var id in newSpeakerIds.Except(currentSpeakerIds).ToList())
        {
            var speaker = conference.Speakers.FirstOrDefault(s => s.Id == id)
                ?? throw new KeyNotFoundException($"Speaker with ID {id} not found in conference");
            presentation.AddSpeaker(speaker);
        }
        foreach (var id in currentSpeakerIds.Except(newSpeakerIds).ToList())
        {
            presentation.RemoveSpeaker(id);
        }

        conference.UpdatePresentation(presentation);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Presentation {PresentationId} updated in conference {ConferenceId}", command.PresentationId, command.ConferenceId);

        return new ConferencePresentationDto(
            presentation.Id, presentation.Title, presentation.Abstract,
            presentation.StartDateTime, presentation.EndDateTime,
            presentation.Room.Id, presentation.Room.Name,
            presentation.Speakers.Select(s => s.Id).ToList(),
            presentation.Speakers.Select(s => new ConferenceSpeakerDto(s.Id, s.Name, s.Company, s.ProfilePictureUrl, s.ExternalId)).ToList(),
            presentation.ExternalId);
    }
}
