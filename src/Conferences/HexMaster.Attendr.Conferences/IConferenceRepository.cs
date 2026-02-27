using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.DomainModels;

namespace HexMaster.Attendr.Conferences;

/// <summary>
/// Repository interface for conference aggregate root operations.
/// </summary>
public interface IConferenceRepository
{
    /// <summary>
    /// Adds a new conference to the repository.
    /// </summary>
    /// <param name="conference">The conference to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(Conference conference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a conference by its ID as a domain model (for write operations).
    /// </summary>
    /// <param name="id">The conference ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conference if found; otherwise, null.</returns>
    Task<Conference?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves conference details by its ID as a DTO (for read operations).
    /// Returns the conference if visible OR if owned by the requesting user.
    /// </summary>
    /// <param name="id">The conference ID.</param>
    /// <param name="currentProfileId">Optional profile ID of the requesting user for owner visibility override.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conference details DTO if found and accessible; otherwise, null.</returns>
    Task<ConferenceDetailsDto?> GetDetailsByIdAsync(Guid id, Guid? currentProfileId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing conference in the repository.
    /// </summary>
    /// <param name="conference">The conference to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(Conference conference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a conference from the repository.
    /// </summary>
    /// <param name="id">The ID of the conference to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the conference was deleted; false if not found.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a presentation by conference ID and presentation ID.
    /// Includes speakers and topics.
    /// </summary>
    /// <param name="conferenceId">The conference ID.</param>
    /// <param name="presentationId">The presentation ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The presentation if found; otherwise, null.</returns>
    Task<Presentation?> GetPresentationByIdAsync(Guid conferenceId, Guid presentationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists conference IDs for conferences that have not ended and have a synchronization source configured.
    /// Useful for periodic background synchronization jobs.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Collection of conference IDs.</returns>
    Task<IReadOnlyCollection<Guid>> ListActiveConferenceIdsWithSyncSourceAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists conferences with optional search and pagination.
    /// Only returns conferences that have not ended (EndDate >= today).
    /// Always includes conferences owned by currentProfileId regardless of visibility.
    /// </summary>
    /// <param name="searchQuery">Optional search query to filter by title, city, or country.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="showHidden">Whether to include hidden conferences (IsVisible = false). Defaults to false.</param>
    /// <param name="currentProfileId">Optional profile ID of the requesting user to apply owner visibility override.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing the list of conferences and the total count.</returns>
    Task<(List<Conference> Conferences, int TotalCount)> ListConferencesAsync(
        string? searchQuery,
        int pageNumber,
        int pageSize,
        bool showHidden = false,
        Guid? currentProfileId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns aggregate metrics about visible conferences, presentations, speakers, rooms, and topics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A DTO containing the metrics.</returns>
    Task<ConferenceMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);
}
