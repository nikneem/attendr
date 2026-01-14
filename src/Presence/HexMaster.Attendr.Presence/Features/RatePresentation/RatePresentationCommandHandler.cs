using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Presence.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Presence.Features.RatePresentation;

/// <summary>
/// Command handler to rate a presentation and optionally mark it as a favorite.
/// Applies business rules for rating (0-5) and updates the presentation presence.
/// </summary>
public sealed class RatePresentationCommandHandler : ICommandHandler<RatePresentationCommand>
{
    private readonly IPresentationPresenceRepository _repository;
    private readonly PresenceMetrics _metrics;
    private readonly ILogger<RatePresentationCommandHandler> _logger;

    public RatePresentationCommandHandler(
        IPresentationPresenceRepository repository,
        PresenceMetrics metrics,
        ILogger<RatePresentationCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(RatePresentationCommand command, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Presence.StartActivity("RatePresentation", ActivityKind.Internal);
        activity?.SetTag("presence.presentation_id", command.PresentationId);
        activity?.SetTag("presence.profile_id", command.ProfileId);
        activity?.SetTag("presence.conference_id", command.ConferenceId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            ArgumentNullException.ThrowIfNull(command.RatingDto);

            activity?.SetTag("presence.rating", command.RatingDto.Rating);
            activity?.SetTag("presence.is_favorite", command.RatingDto.IsFavorite);

            _logger.LogInformation(
                "Rating presentation {PresentationId} for profile {ProfileId} - Rating: {Rating}, IsFavorite: {IsFavorite}",
                command.PresentationId,
                command.ProfileId,
                command.RatingDto.Rating,
                command.RatingDto.IsFavorite);

            var presentation = await _repository.GetByIdAsync(
                command.ProfileId,
                command.ConferenceId,
                command.PresentationId,
                cancellationToken);

            if (presentation == null)
            {
                _logger.LogWarning(
                    "Presentation {PresentationId} not found for profile {ProfileId} and conference {ConferenceId}",
                    command.PresentationId,
                    command.ProfileId,
                    command.ConferenceId);
                throw new InvalidOperationException($"Presentation {command.PresentationId} not found for profile {command.ProfileId}");
            }

            presentation.RatePresentation(command.RatingDto.Rating, command.RatingDto.IsFavorite);

            await _repository.UpdateAsync(command.ProfileId, command.ConferenceId, presentation, cancellationToken);

            _logger.LogInformation(
                "Successfully rated presentation {PresentationId} for profile {ProfileId}",
                command.PresentationId,
                command.ProfileId);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordPresentationRated(command.RatingDto.Rating ?? 0, command.RatingDto.IsFavorite);
            _metrics.RecordOperationDuration("RatePresentation", stopwatch.Elapsed.TotalMilliseconds, true);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("RatePresentation", ex.GetType().Name);
            _metrics.RecordOperationDuration("RatePresentation", stopwatch.Elapsed.TotalMilliseconds, false);
            throw;
        }
    }
}
