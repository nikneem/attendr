using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Core.CommandHandlers;

using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.ManagePresentations;

public sealed class CreateConferencePresentationCommandHandler : ICommandHandler<CreateConferencePresentationCommand, ConferencePresentationDto>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<CreateConferencePresentationCommandHandler> _logger;

    public CreateConferencePresentationCommandHandler(IConferenceRepository repository, ILogger<CreateConferencePresentationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConferencePresentationDto> Handle(CreateConferencePresentationCommand command, CancellationToken cancellationToken = default)
    {
        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference {command.ConferenceId} not found.");

        var room = conference.Rooms.FirstOrDefault(r => r.Id == command.RoomId)
            ?? throw new KeyNotFoundException($"Room {command.RoomId} not found in conference {command.ConferenceId}.");

        var speakers = command.SpeakerIds
            .Select(id => conference.Speakers.FirstOrDefault(s => s.Id == id)
                ?? throw new KeyNotFoundException($"Speaker {id} not found in conference {command.ConferenceId}."))
            .ToList();

        var presentation = Presentation.Create(command.Title, command.Abstract, command.StartDateTime, command.EndDateTime, room, speakers);
        conference.AddPresentation(presentation);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Created presentation {PresentationId} for conference {ConferenceId}", presentation.Id, command.ConferenceId);

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
