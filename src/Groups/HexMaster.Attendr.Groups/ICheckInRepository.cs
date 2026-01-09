using HexMaster.Attendr.Groups.DomainModels;

namespace HexMaster.Attendr.Groups;

/// <summary>
/// Repository interface for group check-in operations.
/// </summary>
public interface ICheckInRepository
{
    /// <summary>
    /// Adds a new check-in record.
    /// </summary>
    /// <param name="checkIn">The check-in to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(CheckIn checkIn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a member to an existing check-in.
    /// </summary>
    /// <param name="checkInId">The check-in ID.</param>
    /// <param name="member">The member to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddMemberAsync(Guid checkInId, CheckedInMember member, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a member from a check-in.
    /// </summary>
    /// <param name="checkInId">The check-in ID.</param>
    /// <param name="memberId">The member ID to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RemoveMemberAsync(Guid checkInId, Guid memberId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a check-in by conference and presentation IDs.
    /// </summary>
    /// <param name="conferenceId">The conference ID.</param>
    /// <param name="presentationId">The presentation ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The check-in if found; otherwise, null.</returns>
    Task<CheckIn?> GetByConferenceAndPresentationAsync(Guid conferenceId, Guid presentationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active check-ins for a conference.
    /// </summary>
    /// <param name="conferenceId">The conference ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of active check-ins.</returns>
    Task<IReadOnlyCollection<CheckIn>> GetActiveByConferenceAsync(Guid conferenceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes expired check-ins.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of deleted check-ins.</returns>
    Task<int> DeleteExpiredAsync(CancellationToken cancellationToken = default);
}
