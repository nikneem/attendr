using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.Speakers.ListSpeakers;

public sealed class ListSpeakersQueryHandler : IQueryHandler<ListSpeakersQuery, IReadOnlyList<ConferenceSpeakerDto>>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<ListSpeakersQueryHandler> _logger;

    public ListSpeakersQueryHandler(IConferenceRepository repository, ILogger<ListSpeakersQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ConferenceSpeakerDto>> Handle(ListSpeakersQuery query, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Conferences.StartActivity("ListSpeakers", ActivityKind.Internal);
        activity?.SetTag("conference.id", query.ConferenceId);

        var conference = await _repository.GetByIdAsync(query.ConferenceId, cancellationToken);
        if (conference == null)
        {
            _logger.LogWarning("Conference {ConferenceId} not found", query.ConferenceId);
            throw new KeyNotFoundException($"Conference with ID {query.ConferenceId} not found");
        }

        return conference.Speakers
            .Select(s => new ConferenceSpeakerDto(s.Id, s.Name, s.Company, s.ProfilePictureUrl, s.ExternalId))
            .ToList();
    }
}
