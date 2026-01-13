using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Presence.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Presence.Features.GetConferenceWithPresentations;

/// <summary>
/// Query handler to retrieve conference details with presentations for a profile.
/// </summary>
public sealed class GetConferenceWithPresentationsQueryHandler : IQueryHandler<GetConferenceWithPresentationsQuery, ConferenceWithPresentationsResponse>
{
    private readonly IConferencePresenceRepository _repository;
    private readonly PresenceMetrics _metrics;
    private readonly ILogger<GetConferenceWithPresentationsQueryHandler> _logger;

    public GetConferenceWithPresentationsQueryHandler(
        IConferencePresenceRepository repository,
        PresenceMetrics metrics,
        ILogger<GetConferenceWithPresentationsQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ConferenceWithPresentationsResponse> Handle(GetConferenceWithPresentationsQuery query, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Presence.StartActivity("GetConferenceWithPresentations", ActivityKind.Internal);
        activity?.SetTag("presence.profile_id", query.ProfileId);
        activity?.SetTag("presence.conference_id", query.ConferenceId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Getting conference with presentations for profile {ProfileId} and conference {ConferenceId}",
                query.ProfileId,
                query.ConferenceId);

            // Get conference presence including all presentations
            var conferencePresence = await _repository.GetAsync(query.ConferenceId, query.ProfileId, cancellationToken);

            if (conferencePresence == null)
            {
                activity?.SetTag("presence.found", false);
                activity?.SetStatus(ActivityStatusCode.Error, "Conference presence not found");

                _logger.LogWarning(
                    "Conference presence not found for profile {ProfileId} and conference {ConferenceId}",
                    query.ProfileId,
                    query.ConferenceId);

                throw new InvalidOperationException($"Conference presence not found for conference {query.ConferenceId} and profile {query.ProfileId}");
            }

            // Map presentations
            var presentations = conferencePresence.Presentations.Select(p => new PresentationPresenceResponse(
                p.PresentationId,
                p.Title,
                p.Abstract,
                p.Room,
                p.StartDateTime,
                p.EndDateTime,
                p.Speakers.Select(s => new SpeakerResponse(s.SpeakerId, s.Name, s.ProfilePictureUrl)).ToList(),
                p.IsFavorite,
                p.IsRecommended,
                p.IsPreferred,
                p.IsRated,
                p.IsCheckedIn,
                p.Rating
            )).ToList();

            var response = new ConferenceWithPresentationsResponse(
                conferencePresence.ConferenceId,
                conferencePresence.ConferenceName,
                conferencePresence.Location,
                conferencePresence.ImageUrl,
                conferencePresence.StartDate,
                conferencePresence.EndDate,
                conferencePresence.IsFollowing,
                conferencePresence.IsAttending,
                presentations
            );

            activity?.SetTag("presence.found", true);
            activity?.SetTag("presence.presentation_count", presentations.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _metrics.RecordOperationDuration("GetConferenceWithPresentations", stopwatch.Elapsed.TotalMilliseconds, true);

            _logger.LogInformation(
                "Successfully retrieved conference with {PresentationCount} presentations for profile {ProfileId}",
                presentations.Count,
                query.ProfileId);

            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _metrics.RecordOperationDuration("GetConferenceWithPresentations", stopwatch.Elapsed.TotalMilliseconds, false);

            _logger.LogError(ex,
                "Error getting conference with presentations for profile {ProfileId} and conference {ConferenceId}",
                query.ProfileId,
                query.ConferenceId);

            throw;
        }
    }
}
