using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.Speakers.GetSpeaker;

public sealed class GetSpeakerQueryHandler : IQueryHandler<GetSpeakerQuery, ConferenceSpeakerDto?>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<GetSpeakerQueryHandler> _logger;

    public GetSpeakerQueryHandler(IConferenceRepository repository, ILogger<GetSpeakerQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConferenceSpeakerDto?> Handle(GetSpeakerQuery query, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Conferences.StartActivity("GetSpeaker", ActivityKind.Internal);
        activity?.SetTag("conference.id", query.ConferenceId);
        activity?.SetTag("speaker.id", query.SpeakerId);

        var conference = await _repository.GetByIdAsync(query.ConferenceId, cancellationToken);
        if (conference == null)
        {
            _logger.LogWarning("Conference {ConferenceId} not found", query.ConferenceId);
            throw new KeyNotFoundException($"Conference with ID {query.ConferenceId} not found");
        }

        var speaker = conference.Speakers.FirstOrDefault(s => s.Id == query.SpeakerId);
        if (speaker == null) return null;
        return new ConferenceSpeakerDto(speaker.Id, speaker.Name, speaker.Company, speaker.ProfilePictureUrl, speaker.ExternalId);
    }
}
