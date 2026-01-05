using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Presence.Services;

namespace HexMaster.Attendr.Presence.Api.Features.RatePresentation;

public sealed class RatePresentationService
{
    private readonly IPresentationPresenceRepository _repository;
    private readonly ILogger<RatePresentationService> _logger;

    public RatePresentationService(
        IPresentationPresenceRepository repository,
        ILogger<RatePresentationService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(
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
