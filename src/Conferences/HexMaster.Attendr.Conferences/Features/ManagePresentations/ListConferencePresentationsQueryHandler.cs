using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;

using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.ManagePresentations;

public sealed class ListConferencePresentationsQueryHandler : IQueryHandler<ListConferencePresentationsQuery, List<ConferencePresentationDto>>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<ListConferencePresentationsQueryHandler> _logger;

    public ListConferencePresentationsQueryHandler(IConferenceRepository repository, ILogger<ListConferencePresentationsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<ConferencePresentationDto>> Handle(ListConferencePresentationsQuery query, CancellationToken cancellationToken = default)
    {
        var conference = await _repository.GetByIdAsync(query.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference {query.ConferenceId} not found.");

        _logger.LogInformation("Listing presentations for conference {ConferenceId}", query.ConferenceId);

        return conference.Presentations.Select(p => new ConferencePresentationDto(
            p.Id,
            p.Title,
            p.Abstract,
            p.StartDateTime,
            p.EndDateTime,
            p.Room.Id,
            p.Room.Name,
            p.Speakers.Select(s => s.Id).ToList(),
            p.Speakers.Select(s => new ConferenceSpeakerDto(s.Id, s.Name, s.Company, s.ProfilePictureUrl)).ToList()
        )).ToList();
    }
}
