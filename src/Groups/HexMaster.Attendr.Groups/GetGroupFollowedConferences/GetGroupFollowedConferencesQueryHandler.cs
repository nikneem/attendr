using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Groups.GetGroupDetails;
using HexMaster.Attendr.Groups.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Groups.GetGroupFollowedConferences;

public sealed class GetGroupFollowedConferencesQueryHandler
    : IQueryHandler<GetGroupFollowedConferencesQuery, IReadOnlyCollection<FollowedConferenceDto>>
{
    private readonly IGroupRepository _groupRepository;
    private readonly GroupMetrics _metrics;
    private readonly ILogger<GetGroupFollowedConferencesQueryHandler> _logger;

    public GetGroupFollowedConferencesQueryHandler(
        IGroupRepository groupRepository,
        GroupMetrics metrics,
        ILogger<GetGroupFollowedConferencesQueryHandler> logger)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyCollection<FollowedConferenceDto>> Handle(
        GetGroupFollowedConferencesQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        using var activity = ActivitySources.Groups.StartActivity("GetGroupFollowedConferences", ActivityKind.Internal);
        activity?.SetTag("group.id", query.GroupId);
        activity?.SetTag("requesting_profile.id", query.RequestingProfileId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var group = await _groupRepository.GetByIdAsync(query.GroupId, cancellationToken);

            if (group is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Group not found");
                _metrics.RecordOperationFailed("GetGroupFollowedConferences", "GroupNotFound");
                _metrics.RecordOperationDuration("GetGroupFollowedConferences", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Attempted to get followed conferences for non-existent group {GroupId}", query.GroupId);
                throw new InvalidOperationException("Group not found.");
            }

            activity?.SetTag("group.name", group.Name);

            var requestingMember = group.Members.FirstOrDefault(m => m.Id == query.RequestingProfileId);
            if (requestingMember is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Requesting user is not a member");
                _metrics.RecordOperationFailed("GetGroupFollowedConferences", "NotAMember");
                _metrics.RecordOperationDuration("GetGroupFollowedConferences", stopwatch.Elapsed.TotalMilliseconds, success: false);

                _logger.LogWarning("Profile {ProfileId} is not a member of group {GroupId}",
                    query.RequestingProfileId, query.GroupId);
                throw new InvalidOperationException("You are not a member of this group.");
            }

            var conferences = group.GetCurrentAndFutureFollowedConferences();

            var dtos = conferences.Select(fc => new FollowedConferenceDto(
                fc.ConferenceId,
                fc.Name,
                fc.GetLocation(),
                fc.StartDate,
                fc.EndDate,
                fc.ImageUrl,
                fc.SpeakersCount,
                fc.SessionsCount)).ToList();

            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("conferences.count", dtos.Count);
            _metrics.RecordOperationDuration("GetGroupFollowedConferences", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Retrieved {Count} followed conferences for group {GroupId}",
                dtos.Count, query.GroupId);

            return dtos.AsReadOnly();
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("GetGroupFollowedConferences", ex.GetType().Name);
            _metrics.RecordOperationDuration("GetGroupFollowedConferences", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to get followed conferences for group {GroupId}", query.GroupId);
            throw;
        }
    }
}
