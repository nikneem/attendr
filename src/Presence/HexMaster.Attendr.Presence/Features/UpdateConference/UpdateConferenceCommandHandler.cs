using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Presence.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Presence.Features.UpdateConference;

/// <summary>
/// Command handler to update all conference presence records when conference details change.
/// This ensures all profiles following/attending a conference have the latest conference information.
/// </summary>
public sealed class UpdateConferenceCommandHandler : ICommandHandler<UpdateConferenceCommand>
{
    private readonly IConferencePresenceRepository _repository;
    private readonly PresenceMetrics _metrics;
    private readonly ILogger<UpdateConferenceCommandHandler> _logger;

    public UpdateConferenceCommandHandler(
        IConferencePresenceRepository repository,
        PresenceMetrics metrics,
        ILogger<UpdateConferenceCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(UpdateConferenceCommand command, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Presence.StartActivity("UpdateConference", ActivityKind.Internal);
        activity?.SetTag("presence.conference_id", command.Event.ConferenceId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(command.Event);

            var conferenceId = command.Event.ConferenceId;
            var location = $"{command.Event.City}, {command.Event.Country}";

            _logger.LogInformation(
                "Updating conference presence records for conference {ConferenceId}: {Title}",
                conferenceId,
                command.Event.Title);

            // Get all conference presences for this conference across all profiles
            var presences = await _repository.GetByConferenceIdAsync(conferenceId, cancellationToken);

            if (presences.Count == 0)
            {
                _logger.LogInformation(
                    "No conference presence records found for conference {ConferenceId}",
                    conferenceId);
                activity?.SetTag("presence.records_updated", 0);
                activity?.SetStatus(ActivityStatusCode.Ok);
                return;
            }

            activity?.SetTag("presence.records_found", presences.Count);

            // Update each conference presence with the new details
            var updateTasks = new List<Task>();
            foreach (var presence in presences)
            {
                presence.UpdateConferenceDetails(
                    command.Event.Title,
                    location,
                    command.Event.StartDate,
                    command.Event.EndDate,
                    command.Event.ImageUrl);

                updateTasks.Add(_repository.UpdateAsync(presence, cancellationToken));
            }

            // Execute all updates in parallel
            await Task.WhenAll(updateTasks);

            stopwatch.Stop();
            _metrics.RecordOperationDuration("UpdateConference", stopwatch.Elapsed.TotalMilliseconds, true);
            activity?.SetTag("presence.records_updated", presences.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(
                "Successfully updated {Count} conference presence records for conference {ConferenceId} in {ElapsedMs}ms",
                presences.Count,
                conferenceId,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _metrics.RecordOperationDuration("UpdateConference", stopwatch.Elapsed.TotalMilliseconds, false);

            _logger.LogError(ex,
                "Error updating conference presence records for conference {ConferenceId}",
                command.Event.ConferenceId);

            throw;
        }
    }
}
