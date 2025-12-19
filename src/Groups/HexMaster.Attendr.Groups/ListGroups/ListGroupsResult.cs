namespace HexMaster.Attendr.Groups.ListGroups;

/// <summary>
/// Result of listing groups with pagination information.
/// </summary>
/// <param name="Groups">The collection of groups for the current page.</param>
/// <param name="TotalCount">The total number of groups matching the search criteria.</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="PageNumber">The current page number (1-based).</param>
public sealed record ListGroupsResult(
    IReadOnlyCollection<GroupListItemDto> Groups,
    int TotalCount,
    int PageSize,
    int PageNumber);
