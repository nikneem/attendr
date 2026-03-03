using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.Presentations.GetPresentation;

public sealed class GetPresentationQueryHandler : IQueryHandler<GetPresentationQuery, ConferencePresentationDto?>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<GetPresentationQueryHandler> _logger;

    public GetPresentationQueryHandler(IConferenceRepository repository, ILogger<GetPresentationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConferencePresentationDto?> Handle(GetPresentationQuery query, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Conferences.StartActivity("GetPresentation", ActivityKind.Internal);
        activity?.SetTag("conference.id", query.ConferenceId);
        activity?.SetTag("presentation.id", query.PresentationId);

        var conference = await _repository.GetByIdAsync(query.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference with ID {query.ConferenceId} not found");

        var p = conference.Presentations.FirstOrDefault(p => p.Id == query.PresentationId);
        if (p == null) return null;

        return new ConferencePresentationDto(
            p.Id, p.Title, p.Abstract, p.StartDateTime, p.EndDateTime,
            p.Room.Id, p.Room.Name,
            p.Speakers.Select(s => s.Id).ToList(),
            p.Speakers.Select(s => new ConferenceSpeakerDto(s.Id, s.Name, s.Company, s.ProfilePictureUrl, s.ExternalId)).ToList(),
            p.ExternalId);
    }
}
