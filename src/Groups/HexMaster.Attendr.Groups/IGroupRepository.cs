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
}
