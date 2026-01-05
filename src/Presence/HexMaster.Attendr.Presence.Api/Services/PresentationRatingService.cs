using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Presence.Services;

namespace HexMaster.Attendr.Presence.Api.Services;

public interface IPresentationRatingService
{
    Task<PresentationToRateDto?> GetRandomUnratedPresentationAsync(
        Guid profileId,
        Guid conferenceId,
        CancellationToken cancellationToken = default);

    Task RatePresentationAsync(
        Guid profileId,
        Guid conferenceId,
        Guid presentationId,
        RatePresentationDto ratingDto,
        CancellationToken cancellationToken = default);
}

public sealed class PresentationRatingService : IPresentationRatingService
{
    private readonly IPresentationPresenceRepository _repository;
    private readonly ILogger<PresentationRatingService> _logger;

    public PresentationRatingService(
        IPresentationPresenceRepository repository,
        ILogger<PresentationRatingService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PresentationToRateDto?> GetRandomUnratedPresentationAsync(
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

    public async Task RatePresentationAsync(
        Guid profileId,
        Guid conferenceId,
        Guid presentationId,
        RatePresentationDto ratingDto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ratingDto);

        _logger.LogInformation(
            "Rating presentation {PresentationId} for profile {ProfileId} - Rating: {Rating}, IsFavorite: {IsFavorite}",
            presentationId,
            profileId,
            ratingDto.Rating,
            ratingDto.IsFavorite);

        var presentation = await _repository.GetByIdAsync(
            profileId,
            conferenceId,
            presentationId,
            cancellationToken);

        if (presentation == null)
        {
            _logger.LogWarning(
                "Presentation {PresentationId} not found for profile {ProfileId} and conference {ConferenceId}",
                presentationId,
                profileId,
                conferenceId);
            throw new InvalidOperationException($"Presentation {presentationId} not found for profile {profileId}");
        }

        presentation.RatePresentation(ratingDto.Rating, ratingDto.IsFavorite);

        await _repository.UpdateAsync(profileId, conferenceId, presentation, cancellationToken);

        _logger.LogInformation(
            "Successfully rated presentation {PresentationId} for profile {ProfileId}",
            presentationId,
            profileId);
    }
}
