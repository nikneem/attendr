using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.Presentations.ListPresentations;

public sealed class ListPresentationsQueryHandler : IQueryHandler<ListPresentationsQuery, IReadOnlyList<ConferencePresentationDto>>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<ListPresentationsQueryHandler> _logger;

    public ListPresentationsQueryHandler(IConferenceRepository repository, ILogger<ListPresentationsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ConferencePresentationDto>> Handle(ListPresentationsQuery query, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Conferences.StartActivity("ListPresentations", ActivityKind.Internal);
        activity?.SetTag("conference.id", query.ConferenceId);

        var conference = await _repository.GetByIdAsync(query.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference with ID {query.ConferenceId} not found");

        return conference.Presentations
            .Select(p => new ConferencePresentationDto(
                p.Id,
                p.Title,
                p.Abstract,
                p.StartDateTime,
                p.EndDateTime,
                p.Room.Id,
                p.Room.Name,
                p.Speakers.Select(s => s.Id).ToList(),
                p.Speakers.Select(s => new ConferenceSpeakerDto(s.Id, s.Name, s.Company, s.ProfilePictureUrl, s.ExternalId)).ToList(),
                p.ExternalId))
            .ToList();
    }
}
