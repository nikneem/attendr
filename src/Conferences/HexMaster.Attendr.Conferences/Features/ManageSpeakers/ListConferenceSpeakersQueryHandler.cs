using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;

using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.ManageSpeakers;

public sealed class ListConferenceSpeakersQueryHandler : IQueryHandler<ListConferenceSpeakersQuery, List<ConferenceSpeakerDto>>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<ListConferenceSpeakersQueryHandler> _logger;

    public ListConferenceSpeakersQueryHandler(IConferenceRepository repository, ILogger<ListConferenceSpeakersQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<ConferenceSpeakerDto>> Handle(ListConferenceSpeakersQuery query, CancellationToken cancellationToken = default)
    {
        var conference = await _repository.GetByIdAsync(query.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference {query.ConferenceId} not found.");

        _logger.LogInformation("Listing speakers for conference {ConferenceId}", query.ConferenceId);

        return conference.Speakers
            .Select(s => new ConferenceSpeakerDto(s.Id, s.Name, s.Company, s.ProfilePictureUrl))
            .ToList();
    }
}
