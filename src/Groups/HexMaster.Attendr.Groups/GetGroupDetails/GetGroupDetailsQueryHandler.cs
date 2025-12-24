using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Groups.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Groups.GetGroupDetails;

/// <summary>
/// Query handler to retrieve detailed information about a specific group.
/// Implements distributed tracing via OpenTelemetry and structured logging.
/// </summary>
public sealed class GetGroupDetailsQueryHandler : IQueryHandler<GetGroupDetailsQuery, GroupDetailsDto?>
{
    private readonly IGroupRepository _groupRepository;
    private readonly GroupMetrics _metrics;
    private readonly ILogger<GetGroupDetailsQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGroupDetailsQueryHandler"/> class.
    /// </summary>
    /// <param name="groupRepository">The repository for accessing group data.</param>
    /// <param name="metrics">The metrics for recording group operations.</param>
    /// <param name="logger">Logger for recording operation details and errors.</param>
    public GetGroupDetailsQueryHandler(
        IGroupRepository groupRepository,
        GroupMetrics metrics,
        ILogger<GetGroupDetailsQueryHandler> logger)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the GetGroupDetailsQuery.
    /// </summary>
    /// <param name="query">The query containing the group ID and profile ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The group details if found; otherwise, null.</returns>
    public async Task<GroupDetailsDto?> Handle(
        GetGroupDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        using var activity = ActivitySources.Groups.StartActivity("GetGroupDetails", ActivityKind.Internal);
        activity?.SetTag("group.id", query.GroupId);
        activity?.SetTag("profile.id", query.ProfileId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get group from repository
            var group = await _groupRepository.GetByIdAsync(query.GroupId, cancellationToken);

            if (group is null)
            {
                activity?.SetStatus(ActivityStatusCode.Ok);
                activity?.SetTag("group.found", false);
                _metrics.RecordGroupQueried(found: false);
                _metrics.RecordOperationDuration("GetGroupDetails", stopwatch.Elapsed.TotalMilliseconds, success: true);

                _logger.LogInformation("Group {GroupId} not found", query.GroupId);
                return null;
            }

            activity?.SetTag("group.found", true);
            activity?.SetTag("group.name", group.Name);
            activity?.SetTag("group.member_count", group.Members.Count);

            // Get current member role
            var currentMember = group.Members.FirstOrDefault(m => m.Id == query.ProfileId);
            var currentMemberRole = currentMember?.Role;

            // Map members to DTOs
            var members = group.Members
                .Select(m => new GetGroupDetailsMemberDto(m.Id, m.Name, m.Role))
                .ToList();

            // Map invitations to DTOs
            var invitations = group.Invitations
                .Select(i => new GroupInvitationDto(i.Id, i.Name, i.ExpirationDate))
                .ToList();

            // Map join requests to DTOs
            var joinRequests = group.JoinRequests
                .Select(jr => new GroupJoinRequestDto(jr.Id, jr.Name, jr.RequestedAt))
                .ToList();

            // Map followed conferences to DTOs (only current and future conferences)
            var followedConferences = group.GetCurrentAndFutureFollowedConferences()
                .Select(fc => new FollowedConferenceDto(
                    fc.ConferenceId,
                    fc.Name,
                    fc.GetLocation(),
                    fc.StartDate,
                    fc.EndDate,
                    fc.ImageUrl,
                    fc.SpeakersCount,
                    fc.SessionsCount))
                .ToList();

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordGroupQueried(found: true);
            _metrics.RecordOperationDuration("GetGroupDetails", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Retrieved group {GroupId}: {Name} with {MemberCount} members",
                group.Id, group.Name, members.Count);

            // Map to DTO with membership information
            return new GroupDetailsDto(
                group.Id,
                group.Name,
                group.Members.Count,
                currentMember != null,
                group.Settings.IsPublic,
                currentMemberRole,
                members,
                invitations,
                joinRequests,
                followedConferences);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("GetGroupDetails", ex.GetType().Name);
            _metrics.RecordOperationDuration("GetGroupDetails", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to retrieve group {GroupId}", query.GroupId);
            throw;
        }
    }
}
