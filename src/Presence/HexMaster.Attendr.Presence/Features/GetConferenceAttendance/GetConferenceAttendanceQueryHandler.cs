using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Presence.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Presence.Features.GetConferenceAttendance;

/// <summary>
/// Query handler to retrieve conference attendance information.
/// </summary>
public sealed class GetConferenceAttendanceQueryHandler : IQueryHandler<GetConferenceAttendanceQuery, ConferenceAttendanceDto>
{
    private readonly IConferencePresenceRepository _conferenceRepository;
    private readonly IPresentationPresenceRepository _presentationRepository;
    private readonly PresenceMetrics _metrics;
    private readonly ILogger<GetConferenceAttendanceQueryHandler> _logger;

    public GetConferenceAttendanceQueryHandler(
        IConferencePresenceRepository conferenceRepository,
        IPresentationPresenceRepository presentationRepository,
        PresenceMetrics metrics,
        ILogger<GetConferenceAttendanceQueryHandler> logger)
    {
        _conferenceRepository = conferenceRepository ?? throw new ArgumentNullException(nameof(conferenceRepository));
        _presentationRepository = presentationRepository ?? throw new ArgumentNullException(nameof(presentationRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ConferenceAttendanceDto> Handle(GetConferenceAttendanceQuery query, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Presence.StartActivity("GetConferenceAttendance", ActivityKind.Internal);
        activity?.SetTag("presence.profile_id", query.ProfileId);
        activity?.SetTag("presence.conference_id", query.ConferenceId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Getting conference attendance for profile {ProfileId} and conference {ConferenceId}",
                query.ProfileId,
                query.ConferenceId);

            // Get conference presence
            var conferencePresence = await _conferenceRepository.GetAsync(query.ConferenceId, query.ProfileId, cancellationToken);

            if (conferencePresence == null)
            {
                // Profile is not following this conference
                activity?.SetTag("presence.is_following", false);
                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogInformation(
                    "Profile {ProfileId} is not following conference {ConferenceId}",
                    query.ProfileId,
                    query.ConferenceId);

                _metrics.RecordOperationDuration("GetConferenceAttendance", stopwatch.Elapsed.TotalMilliseconds, true);

                return new ConferenceAttendanceDto(
                    query.ConferenceId,
                    IsFollowing: false,
                    IsAttending: false,
                    FavoritePresentationIds: Array.Empty<Guid>(),
                    RecommendedPresentationIds: Array.Empty<Guid>());
            }

            // Get favorite presentations
            var presentations = await _presentationRepository.GetByProfileAndConferenceAsync(
                query.ProfileId,
                query.ConferenceId,
                cancellationToken);

            var favoritePresentationIds = presentations
                .Where(p => p.IsFavorite)
                .Select(p => p.PresentationId)
                .ToList()
                .AsReadOnly();

            // Get recommended presentations (top 10 recommended presentations)
            var recommendedPresentationIds = presentations
                .Where(p => p.IsRecommended)
                .OrderByDescending(p => p.IsPreferred)
                .Take(10)
                .Select(p => p.PresentationId)
                .ToList()
                .AsReadOnly();

            activity?.SetTag("presence.is_following", true);
            activity?.SetTag("presence.is_attending", conferencePresence.IsAttending);
            activity?.SetTag("presence.favorites_count", favoritePresentationIds.Count);
            activity?.SetTag("presence.recommended_count", recommendedPresentationIds.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _metrics.RecordOperationDuration("GetConferenceAttendance", stopwatch.Elapsed.TotalMilliseconds, true);

            return new ConferenceAttendanceDto(
                query.ConferenceId,
                IsFollowing: true,
                IsAttending: conferencePresence.IsAttending,
                FavoritePresentationIds: favoritePresentationIds,
                RecommendedPresentationIds: recommendedPresentationIds);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("GetConferenceAttendance", ex.GetType().Name);
            _metrics.RecordOperationDuration("GetConferenceAttendance", stopwatch.Elapsed.TotalMilliseconds, false);
            throw;
        }
    }
}
