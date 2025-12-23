using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Groups.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Groups.GetMyGroups;

/// <summary>
/// Query handler to retrieve all groups where the specified profile is a member.
/// Returns groups sorted alphabetically by name.
/// Implements distributed tracing via OpenTelemetry and structured logging.
/// </summary>
public sealed class GetMyGroupsQueryHandler : IQueryHandler<GetMyGroupsQuery, IReadOnlyCollection<MyGroupDto>>
{
    private readonly IGroupRepository _groupRepository;
    private readonly GroupMetrics _metrics;
    private readonly ILogger<GetMyGroupsQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMyGroupsQueryHandler"/> class.
    /// </summary>
    /// <param name="groupRepository">The repository for accessing group data.</param>
    /// <param name="metrics">The metrics for recording group operations.</param>
    /// <param name="logger">Logger for recording operation details and errors.</param>
    public GetMyGroupsQueryHandler(
        IGroupRepository groupRepository,
        GroupMetrics metrics,
        ILogger<GetMyGroupsQueryHandler> logger)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the GetMyGroupsQuery.
    /// </summary>
    /// <param name="query">The query containing the profile ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of groups where the user is a member, sorted alphabetically.</returns>
    public async Task<IReadOnlyCollection<MyGroupDto>> Handle(
        GetMyGroupsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        using var activity = ActivitySources.Groups.StartActivity("GetMyGroups", ActivityKind.Internal);
        activity?.SetTag("profile.id", query.ProfileId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get all groups where the profile is a member
            var groups = await _groupRepository.GetGroupsByMemberIdAsync(query.ProfileId, cancellationToken);

            // Map to DTOs and sort alphabetically by name
            var result = groups
                .Select(g => new MyGroupDto(
                    g.Id,
                    g.Name,
                    g.Members.Count))
                .OrderBy(g => g.Name)
                .ToList()
                .AsReadOnly();

            activity?.SetTag("groups.count", result.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordMyGroupsQueried(result.Count);
            _metrics.RecordOperationDuration("GetMyGroups", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Retrieved {Count} groups for profile {ProfileId}", result.Count, query.ProfileId);

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("GetMyGroups", ex.GetType().Name);
            _metrics.RecordOperationDuration("GetMyGroups", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to retrieve groups for profile {ProfileId}", query.ProfileId);
            throw;
        }
    }
}
