using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;

using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.ManagePresentations;

public sealed class UpdateConferencePresentationCommandHandler : ICommandHandler<UpdateConferencePresentationCommand, ConferencePresentationDto>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<UpdateConferencePresentationCommandHandler> _logger;

    public UpdateConferencePresentationCommandHandler(IConferenceRepository repository, ILogger<UpdateConferencePresentationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConferencePresentationDto> Handle(UpdateConferencePresentationCommand command, CancellationToken cancellationToken = default)
    {
        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference {command.ConferenceId} not found.");

        var presentation = conference.Presentations.FirstOrDefault(p => p.Id == command.PresentationId)
            ?? throw new KeyNotFoundException($"Presentation {command.PresentationId} not found in conference {command.ConferenceId}.");

        var room = conference.Rooms.FirstOrDefault(r => r.Id == command.RoomId)
            ?? throw new KeyNotFoundException($"Room {command.RoomId} not found in conference {command.ConferenceId}.");

        presentation.UpdateDetails(command.Title, command.Abstract, command.StartDateTime, command.EndDateTime);
        presentation.ChangeRoom(room);

        // Replace speakers: remove all existing, add new ones
        foreach (var existingSpeaker in presentation.Speakers.ToList())
        {
            presentation.RemoveSpeaker(existingSpeaker.Id);
        }
        foreach (var speakerId in command.SpeakerIds)
        {
            var speaker = conference.Speakers.FirstOrDefault(s => s.Id == speakerId)
                ?? throw new KeyNotFoundException($"Speaker {speakerId} not found in conference {command.ConferenceId}.");
            presentation.AddSpeaker(speaker);
        }

        conference.UpdatePresentation(presentation);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Updated presentation {PresentationId} for conference {ConferenceId}", command.PresentationId, command.ConferenceId);

        return new ConferencePresentationDto(
            presentation.Id,
            presentation.Title,
            presentation.Abstract,
            presentation.StartDateTime,
            presentation.EndDateTime,
            presentation.Room.Id,
            presentation.Room.Name,
            presentation.Speakers.Select(s => s.Id).ToList(),
            presentation.Speakers.Select(s => new ConferenceSpeakerDto(s.Id, s.Name, s.Company, s.ProfilePictureUrl)).ToList());
    }
}
