using HexMaster.Attendr.Presence.DomainModels;

namespace HexMaster.Attendr.Presence;

/// <summary>
/// Repository interface for conference presence aggregate root operations.
/// </summary>
public interface IConferencePresenceRepository
{
    /// <summary>
    /// Checks if a conference presence exists for a specific profile and conference.
    /// </summary>
    /// <param name="profileId">The unique identifier of the profile.</param>
    /// <param name="conferenceId">The unique identifier of the conference.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the presence exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(Guid profileId, Guid conferenceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new conference presence to the repository.
    /// </summary>
    /// <param name="presence">The conference presence to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(ConferencePresence presence, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all conference presences for a specific profile.
    /// </summary>
    /// <param name="profileId">The unique identifier of the profile.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only collection of conference presences.</returns>
    Task<IReadOnlyCollection<ConferencePresence>> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all conference presences for a specific conference across all profiles.
    /// </summary>
    /// <param name="conferenceId">The unique identifier of the conference.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only collection of conference presences.</returns>
    Task<IReadOnlyCollection<ConferencePresence>> GetByConferenceIdAsync(Guid conferenceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific conference presence by conference ID and profile ID.
    /// </summary>
    /// <param name="conferenceId">The unique identifier of the conference.</param>
    /// <param name="profileId">The unique identifier of the profile.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conference presence if found; otherwise, null.</returns>
    Task<ConferencePresence?> GetAsync(Guid conferenceId, Guid profileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing conference presence.
    /// </summary>
    /// <param name="presence">The conference presence to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(ConferencePresence presence, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a conference presence for a specific profile and conference.
    /// </summary>
    /// <param name="conferenceId">The unique identifier of the conference.</param>
    /// <param name="profileId">The unique identifier of the profile.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteAsync(Guid conferenceId, Guid profileId, CancellationToken cancellationToken = default);
}
