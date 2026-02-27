using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.Presentations.CreatePresentation;

public sealed class CreatePresentationCommandHandler : ICommandHandler<CreatePresentationCommand, ConferencePresentationDto>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<CreatePresentationCommandHandler> _logger;

    public CreatePresentationCommandHandler(IConferenceRepository repository, ILogger<CreatePresentationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConferencePresentationDto> Handle(CreatePresentationCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        using var activity = ActivitySources.Conferences.StartActivity("CreatePresentation", ActivityKind.Internal);
        activity?.SetTag("conference.id", command.ConferenceId);

        if (command.SpeakerIds == null || command.SpeakerIds.Count == 0)
            throw new ArgumentException("At least one speaker is required.", nameof(command.SpeakerIds));

        if (command.EndDateTime <= command.StartDateTime)
            throw new ArgumentException("End date/time must be after start date/time.", nameof(command.EndDateTime));

        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference with ID {command.ConferenceId} not found");

        if (!command.IsAdmin && conference.CreatedByProfileId != command.RequestingProfileId)
            throw new ForbiddenException("You are not authorized to modify this conference.", "https://attendr.dev/errors/forbidden");

        var room = conference.Rooms.FirstOrDefault(r => r.Id == command.RoomId)
            ?? throw new KeyNotFoundException($"Room with ID {command.RoomId} not found in conference");

        var speakers = command.SpeakerIds
            .Select(id => conference.Speakers.FirstOrDefault(s => s.Id == id)
                ?? throw new KeyNotFoundException($"Speaker with ID {id} not found in conference"))
            .ToList();

        var presentation = Presentation.Create(command.Title, command.Abstract, command.StartDateTime, command.EndDateTime, room, speakers);
        conference.AddPresentation(presentation);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Presentation {PresentationId} created for conference {ConferenceId}", presentation.Id, command.ConferenceId);

        return new ConferencePresentationDto(
            presentation.Id, presentation.Title, presentation.Abstract,
            presentation.StartDateTime, presentation.EndDateTime,
            presentation.Room.Id, presentation.Room.Name,
            presentation.Speakers.Select(s => s.Id).ToList(),
            presentation.Speakers.Select(s => new ConferenceSpeakerDto(s.Id, s.Name, s.Company, s.ProfilePictureUrl, s.ExternalId)).ToList(),
            presentation.ExternalId);
    }
}
