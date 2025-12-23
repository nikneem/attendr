using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Groups.Observability;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Groups.ListGroups;

/// <summary>
/// Query handler to retrieve a paginated list of all groups with optional search filtering.
/// Returns groups with information about whether the current profile is a member.
/// Implements distributed tracing via OpenTelemetry and structured logging.
/// </summary>
public sealed class ListGroupsQueryHandler : IQueryHandler<ListGroupsQuery, ListGroupsResult>
{
    private readonly IGroupRepository _groupRepository;
    private readonly GroupMetrics _metrics;
    private readonly ILogger<ListGroupsQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListGroupsQueryHandler"/> class.
    /// </summary>
    /// <param name="groupRepository">The repository for accessing group data.</param>
    /// <param name="metrics">The metrics for recording group operations.</param>
    /// <param name="logger">Logger for recording operation details and errors.</param>
    public ListGroupsQueryHandler(
        IGroupRepository groupRepository,
        GroupMetrics metrics,
        ILogger<ListGroupsQueryHandler> logger)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the ListGroupsQuery.
    /// </summary>
    /// <param name="query">The query containing pagination and search parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of groups with total count and membership information.</returns>
    public async Task<ListGroupsResult> Handle(
        ListGroupsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        using var activity = ActivitySources.Groups.StartActivity("ListGroups", ActivityKind.Internal);
        activity?.SetTag("search_query", query.SearchQuery ?? "null");
        activity?.SetTag("page_number", query.PageNumber);
        activity?.SetTag("page_size", query.PageSize);
        activity?.SetTag("profile.id", query.ProfileId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get paginated groups from repository
            var (groups, totalCount) = await _groupRepository.ListGroupsAsync(
                query.SearchQuery,
                query.PageSize,
                query.PageNumber,
                cancellationToken);

            // Map to DTOs with membership information
            var groupDtos = groups
                .Select(g => new GroupListItemDto(
                    g.Id,
                    g.Name,
                    g.Members.Count,
                    g.Members.Any(m => m.Id == query.ProfileId),
                    g.Settings.IsPublic))
                .ToList()
                .AsReadOnly();

            activity?.SetTag("total_count", totalCount);
            activity?.SetTag("returned_count", groupDtos.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordGroupsListed(groupDtos.Count);
            _metrics.RecordOperationDuration("ListGroups", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Listed {Count} groups (page {Page}, total {Total})",
                groupDtos.Count, query.PageNumber, totalCount);

            return new ListGroupsResult(
                groupDtos,
                totalCount,
                query.PageSize,
                query.PageNumber);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("ListGroups", ex.GetType().Name);
            _metrics.RecordOperationDuration("ListGroups", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to list groups");
            throw;
        }
    }
}
