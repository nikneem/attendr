namespace HexMaster.Attendr.Groups.ListGroups;

/// <summary>
/// Query to retrieve a paginated list of all groups with optional search filtering.
/// </summary>
/// <param name="ProfileId">The ID of the profile making the request.</param>
/// <param name="SearchQuery">Optional search term to filter groups by name.</param>
/// <param name="PageSize">The number of groups to return per page.</param>
/// <param name="PageNumber">The page number (1-based).</param>
public sealed record ListGroupsQuery(
    Guid ProfileId,
    string? SearchQuery,
    int PageSize,
    int PageNumber);
