using HexMaster.Attendr.Presence.DomainModels;

namespace HexMaster.Attendr.Presence;

/// <summary>
/// Repository interface for presentation presence operations.
/// </summary>
public interface IPresentationPresenceRepository
{
    /// <summary>
    /// Adds a new presentation presence to the repository.
    /// </summary>
    /// <param name="presentation">The presentation presence to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(PresentationPresence presentation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple presentation presences to the repository in a single bulk operation.
    /// This method is optimized for inserting large numbers of presentation presences efficiently.
    /// </summary>
    /// <param name="presentations">The collection of presentation presences to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddManyAsync(IEnumerable<PresentationPresence> presentations, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all presentation presences for a specific conference and presentation.
    /// </summary>
    /// <param name="conferenceId">The unique identifier of the conference.</param>
    /// <param name="presentationId">The unique identifier of the presentation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only collection of presentation presences.</returns>
    Task<IReadOnlyCollection<PresentationPresence>> GetByConferenceAndPresentationAsync(
        Guid conferenceId,
        Guid presentationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific presentation presence by profile, conference, and presentation.
    /// </summary>
    /// <param name="profileId">The unique identifier of the profile.</param>
    /// <param name="conferenceId">The unique identifier of the conference.</param>
    /// <param name="presentationId">The unique identifier of the presentation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The presentation presence if found; otherwise, null.</returns>
    Task<PresentationPresence?> GetByIdAsync(
        Guid profileId,
        Guid conferenceId,
        Guid presentationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all unrated presentation presences for a specific profile and conference.
    /// </summary>
    /// <param name="profileId">The unique identifier of the profile.</param>
    /// <param name="conferenceId">The unique identifier of the conference.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only collection of unrated presentation presences.</returns>
    Task<IReadOnlyCollection<PresentationPresence>> GetUnratedByProfileAndConferenceAsync(
        Guid profileId,
        Guid conferenceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all presentation presences for a specific profile and conference.
    /// </summary>
    /// <param name="profileId">The unique identifier of the profile.</param>
    /// <param name="conferenceId">The unique identifier of the conference.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A read-only collection of presentation presences.</returns>
    Task<IReadOnlyCollection<PresentationPresence>> GetByProfileAndConferenceAsync(
        Guid profileId,
        Guid conferenceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing presentation presence.
    /// </summary>
    /// <param name="profileId">The unique identifier of the profile.</param>
    /// <param name="conferenceId">The unique identifier of the conference.</param>
    /// <param name="presentation">The presentation presence to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(
        Guid profileId,
        Guid conferenceId,
        PresentationPresence presentation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a specific presentation presence.
    /// </summary>
    /// <param name="profileId">The unique identifier of the profile.</param>
    /// <param name="conferenceId">The unique identifier of the conference.</param>
    /// <param name="presentationId">The unique identifier of the presentation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteAsync(
        Guid profileId,
        Guid conferenceId,
        Guid presentationId,
        CancellationToken cancellationToken = default);
}
