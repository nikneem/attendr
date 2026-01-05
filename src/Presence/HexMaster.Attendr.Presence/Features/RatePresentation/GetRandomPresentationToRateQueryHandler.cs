using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Presence.Services;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Presence.Features.RatePresentation;

/// <summary>
/// Query handler to retrieve a random unrated presentation for rating.
/// Helps users discover presentations they haven't rated yet.
/// </summary>
public sealed class GetRandomPresentationToRateQueryHandler : IQueryHandler<GetRandomPresentationToRateQuery, PresentationToRateDto?>
{
    private readonly IPresentationPresenceRepository _repository;
    private readonly ILogger<GetRandomPresentationToRateQueryHandler> _logger;

    public GetRandomPresentationToRateQueryHandler(
        IPresentationPresenceRepository repository,
        ILogger<GetRandomPresentationToRateQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PresentationToRateDto?> Handle(GetRandomPresentationToRateQuery query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting random unrated presentation for profile {ProfileId} and conference {ConferenceId}",
            query.ProfileId,
            query.ConferenceId);

        var unratedPresentations = await _repository.GetUnratedByProfileAndConferenceAsync(
            query.ProfileId,
            query.ConferenceId,
            cancellationToken);

        if (unratedPresentations.Count == 0)
        {
            _logger.LogInformation(
                "No unrated presentations found for profile {ProfileId} and conference {ConferenceId}",
                query.ProfileId,
                query.ConferenceId);
            return null;
        }

        // Select random presentation
        var random = new Random();
        var randomPresentation = unratedPresentations.ElementAt(random.Next(unratedPresentations.Count));

        return new PresentationToRateDto(
            randomPresentation.PresentationId,
            randomPresentation.Title,
            randomPresentation.Abstract,
            randomPresentation.Room,
            randomPresentation.StartDateTime,
            randomPresentation.EndDateTime);
    }
}
