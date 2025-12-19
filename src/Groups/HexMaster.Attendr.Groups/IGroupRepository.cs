using HexMaster.Attendr.Groups.DomainModels;

namespace HexMaster.Attendr.Groups;

/// <summary>
/// Repository interface for group aggregate root operations.
/// </summary>
public interface IGroupRepository
{
    /// <summary>
    /// Adds a new group to the repository.
    /// </summary>
    /// <param name="group">The group to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(Group group, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a group by its ID.
    /// </summary>
    /// <param name="id">The group ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The group if found; otherwise, null.</returns>
    Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all groups where the specified member is a participant.
    /// </summary>
    /// <param name="memberId">The member's profile ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of groups where the member participates.</returns>
    Task<IReadOnlyCollection<Group>> GetGroupsByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of all groups with optional search filtering.
    /// </summary>
    /// <param name="searchQuery">Optional search term to filter groups by name.</param>
    /// <param name="pageSize">The number of groups to return per page.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing the collection of groups and the total count of matching groups.</returns>
    Task<(IReadOnlyCollection<Group> Groups, int TotalCount)> ListGroupsAsync(
        string? searchQuery,
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default);
}
