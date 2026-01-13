using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Presence.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Presence.Features.SetPreferredPresentation;

/// <summary>
/// Command handler to set a presentation as preferred for a timeslot.
/// Unsets all other favorite presentations in the same timeslot and sets the specified one as preferred.
/// </summary>
public sealed class SetPreferredPresentationCommandHandler : ICommandHandler<SetPreferredPresentationCommand>
{
    private readonly IPresentationPresenceRepository _repository;
    private readonly PresenceMetrics _metrics;
    private readonly ILogger<SetPreferredPresentationCommandHandler> _logger;

    public SetPreferredPresentationCommandHandler(
        IPresentationPresenceRepository repository,
        PresenceMetrics metrics,
        ILogger<SetPreferredPresentationCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(SetPreferredPresentationCommand command, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Presence.StartActivity("SetPreferredPresentation", ActivityKind.Internal);
        activity?.SetTag("presence.presentation_id", command.PresentationId);
        activity?.SetTag("presence.profile_id", command.ProfileId);
        activity?.SetTag("presence.conference_id", command.ConferenceId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Setting presentation {PresentationId} as preferred for profile {ProfileId} in conference {ConferenceId}",
                command.PresentationId,
                command.ProfileId,
                command.ConferenceId);

            // Get the presentation to be set as preferred
            var targetPresentation = await _repository.GetByIdAsync(
                command.ProfileId,
                command.ConferenceId,
                command.PresentationId,
                cancellationToken);

            if (targetPresentation == null)
            {
                _logger.LogWarning(
                    "Presentation {PresentationId} not found for profile {ProfileId} and conference {ConferenceId}",
                    command.PresentationId,
                    command.ProfileId,
                    command.ConferenceId);
                throw new KeyNotFoundException($"Presentation {command.PresentationId} not found");
            }

            if (!targetPresentation.IsFavorite)
            {
                _logger.LogWarning(
                    "Cannot set non-favorite presentation {PresentationId} as preferred",
                    command.PresentationId);
                throw new InvalidOperationException("Cannot set a non-favorite presentation as preferred");
            }

            // Get all presentations for this profile and conference
            var allPresentations = await _repository.GetByProfileAndConferenceAsync(
                command.ProfileId,
                command.ConferenceId,
                cancellationToken);

            // Find all favorite presentations in the same timeslot
            var overlappingFavorites = allPresentations
                .Where(p => p.IsFavorite &&
                           p.PresentationId != command.PresentationId &&
                           p.StartDateTime < targetPresentation.EndDateTime &&
                           p.EndDateTime > targetPresentation.StartDateTime)
                .ToList();

            _logger.LogInformation(
                "Found {Count} overlapping favorite presentations for timeslot",
                overlappingFavorites.Count);

            // Unset preferred for all overlapping favorites
            foreach (var presentation in overlappingFavorites)
            {
                presentation.UnsetAsPreferred();
                await _repository.UpdateAsync(
                    command.ProfileId,
                    command.ConferenceId,
                    presentation,
                    cancellationToken);

                _logger.LogDebug(
                    "Unset preferred for presentation {PresentationId}",
                    presentation.PresentationId);
            }

            // Set the target presentation as preferred
            targetPresentation.SetAsPreferred();
            await _repository.UpdateAsync(
                command.ProfileId,
                command.ConferenceId,
                targetPresentation,
                cancellationToken);

            _logger.LogInformation(
                "Successfully set presentation {PresentationId} as preferred for profile {ProfileId}",
                command.PresentationId,
                command.ProfileId);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("SetPreferredPresentation", stopwatch.Elapsed.TotalMilliseconds, true);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("SetPreferredPresentation", ex.GetType().Name);
            _metrics.RecordOperationDuration("SetPreferredPresentation", stopwatch.Elapsed.TotalMilliseconds, false);
            throw;
        }
    }
}
