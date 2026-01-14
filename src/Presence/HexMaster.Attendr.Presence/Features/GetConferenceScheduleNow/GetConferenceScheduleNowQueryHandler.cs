using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Presence.DomainModels;
using HexMaster.Attendr.Presence.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Presence.Features.GetConferenceScheduleNow;

/// <summary>
/// Query handler to retrieve favorite presentations organized by timeslot (Previous, Now, Next).
/// </summary>
public sealed class GetConferenceScheduleNowQueryHandler : IQueryHandler<GetConferenceScheduleNowQuery, ConferenceScheduleNowResponse>
{
    private readonly IPresentationPresenceRepository _repository;
    private readonly PresenceMetrics _metrics;
    private readonly ILogger<GetConferenceScheduleNowQueryHandler> _logger;

    public GetConferenceScheduleNowQueryHandler(
        IPresentationPresenceRepository repository,
        PresenceMetrics metrics,
        ILogger<GetConferenceScheduleNowQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ConferenceScheduleNowResponse> Handle(GetConferenceScheduleNowQuery query, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Presence.StartActivity("GetConferenceScheduleNow", ActivityKind.Internal);
        activity?.SetTag("presence.profile_id", query.ProfileId);
        activity?.SetTag("presence.conference_id", query.ConferenceId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Retrieving schedule now for profile {ProfileId} and conference {ConferenceId}",
                query.ProfileId,
                query.ConferenceId);

            // Get all presentations for the conference and profile
            var presentations = await _repository.GetByProfileAndConferenceAsync(
                query.ProfileId,
                query.ConferenceId,
                cancellationToken);

            // Filter to only favorite presentations
            var favoritesPresentations = presentations
                .Where(p => p.IsFavorite)
                .OrderBy(p => p.StartDateTime)
                .ToList();

            if (favoritesPresentations.Count == 0)
            {
                _logger.LogInformation(
                    "No favorite presentations found for profile {ProfileId} and conference {ConferenceId}",
                    query.ProfileId,
                    query.ConferenceId);

                stopwatch.Stop();
                _metrics.RecordOperationDuration("GetConferenceScheduleNow", stopwatch.Elapsed.TotalMilliseconds, true);
                activity?.SetStatus(ActivityStatusCode.Ok);

                return new ConferenceScheduleNowResponse(
                    Array.Empty<ScheduledPresentationResponse>(),
                    Array.Empty<ScheduledPresentationResponse>(),
                    Array.Empty<ScheduledPresentationResponse>());
            }

            var now = DateTime.UtcNow;

            // Find current presentations (running now)
            var currentPresentations = favoritesPresentations
                .Where(p => p.StartDateTime <= now && p.EndDateTime > now)
                .ToList();

            // Determine the timeslots
            List<PresentationPresence> previousTimeslot = new();
            List<PresentationPresence> nextTimeslot = new();

            if (currentPresentations.Any())
            {
                // If there are current presentations, find previous and next based on their timeslot
                var currentStart = currentPresentations.First().StartDateTime;
                var currentEnd = currentPresentations.First().EndDateTime;

                // Previous: presentations that ended at or just before the current timeslot started
                previousTimeslot = favoritesPresentations
                    .Where(p => p.EndDateTime <= currentStart)
                    .OrderByDescending(p => p.EndDateTime)
                    .TakeWhile(p => p.EndDateTime == favoritesPresentations
                        .Where(x => x.EndDateTime <= currentStart)
                        .Max(x => x.EndDateTime))
                    .ToList();

                // Next: presentations that start at or just after the current timeslot ends
                nextTimeslot = favoritesPresentations
                    .Where(p => p.StartDateTime >= currentEnd)
                    .OrderBy(p => p.StartDateTime)
                    .TakeWhile(p => p.StartDateTime == favoritesPresentations
                        .Where(x => x.StartDateTime >= currentEnd)
                        .Min(x => x.StartDateTime))
                    .ToList();
            }
            else
            {
                // No current presentations, find the closest future and past presentations
                var futurePresentations = favoritesPresentations
                    .Where(p => p.StartDateTime > now)
                    .ToList();

                var pastPresentations = favoritesPresentations
                    .Where(p => p.EndDateTime <= now)
                    .ToList();

                if (futurePresentations.Any())
                {
                    // Next: presentations in the next timeslot
                    var nextStart = futurePresentations.Min(p => p.StartDateTime);
                    nextTimeslot = futurePresentations
                        .Where(p => p.StartDateTime == nextStart)
                        .ToList();

                    // Previous: last completed timeslot
                    if (pastPresentations.Any())
                    {
                        var previousEnd = pastPresentations.Max(p => p.EndDateTime);
                        previousTimeslot = pastPresentations
                            .Where(p => p.EndDateTime == previousEnd)
                            .ToList();
                    }
                }
                else if (pastPresentations.Any())
                {
                    // All presentations are in the past, show the last timeslot as previous
                    var previousEnd = pastPresentations.Max(p => p.EndDateTime);
                    previousTimeslot = pastPresentations
                        .Where(p => p.EndDateTime == previousEnd)
                        .ToList();
                }
            }

            var response = new ConferenceScheduleNowResponse(
                previousTimeslot.Select(MapToResponse).ToList(),
                currentPresentations.Select(MapToResponse).ToList(),
                nextTimeslot.Select(MapToResponse).ToList());

            stopwatch.Stop();
            _metrics.RecordOperationDuration("GetConferenceScheduleNow", stopwatch.Elapsed.TotalMilliseconds, true);
            activity?.SetTag("presence.previous_count", response.Previous.Count);
            activity?.SetTag("presence.now_count", response.Now.Count);
            activity?.SetTag("presence.next_count", response.Next.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(
                "Retrieved schedule now for profile {ProfileId}: Previous={Previous}, Now={Now}, Next={Next} in {ElapsedMs}ms",
                query.ProfileId,
                response.Previous.Count,
                response.Now.Count,
                response.Next.Count,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _metrics.RecordOperationDuration("GetConferenceScheduleNow", stopwatch.Elapsed.TotalMilliseconds, false);

            _logger.LogError(ex,
                "Error retrieving schedule now for profile {ProfileId} and conference {ConferenceId}",
                query.ProfileId,
                query.ConferenceId);

            throw;
        }
    }

    private static ScheduledPresentationResponse MapToResponse(PresentationPresence presentation)
    {
        return new ScheduledPresentationResponse(
            presentation.PresentationId,
            presentation.Title,
            presentation.Abstract,
            presentation.Room,
            presentation.StartDateTime,
            presentation.EndDateTime,
            presentation.Speakers.Select(s => new ScheduledSpeakerResponse(
                s.SpeakerId,
                s.Name,
                s.ProfilePictureUrl)).ToList(),
            presentation.IsPreferred);
    }
}
