using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Groups.ListGroups;

/// <summary>
/// Query handler to retrieve a paginated list of all groups with optional search filtering.
/// Returns groups with information about whether the current profile is a member.
/// </summary>
public sealed class ListGroupsQueryHandler : IQueryHandler<ListGroupsQuery, ListGroupsResult>
{
    private readonly IGroupRepository _groupRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListGroupsQueryHandler"/> class.
    /// </summary>
    /// <param name="groupRepository">The repository for accessing group data.</param>
    public ListGroupsQueryHandler(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
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

        return new ListGroupsResult(
            groupDtos,
            totalCount,
            query.PageSize,
            query.PageNumber);
    }
}
