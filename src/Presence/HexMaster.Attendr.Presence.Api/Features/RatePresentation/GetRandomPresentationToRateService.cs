using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Presence.Services;

namespace HexMaster.Attendr.Presence.Api.Features.RatePresentation;

public sealed class GetRandomPresentationToRateService
{
    private readonly IPresentationPresenceRepository _repository;
    private readonly ILogger<GetRandomPresentationToRateService> _logger;

    public GetRandomPresentationToRateService(
        IPresentationPresenceRepository repository,
        ILogger<GetRandomPresentationToRateService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PresentationToRateDto?> ExecuteAsync(
        Guid profileId,
        Guid conferenceId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting random unrated presentation for profile {ProfileId} and conference {ConferenceId}",
            profileId,
            conferenceId);

        var unratedPresentations = await _repository.GetUnratedByProfileAndConferenceAsync(
            profileId,
            conferenceId,
            cancellationToken);

        if (unratedPresentations.Count == 0)
        {
            _logger.LogInformation(
                "No unrated presentations found for profile {ProfileId} and conference {ConferenceId}",
                profileId,
                conferenceId);
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
