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
    /// Retrieves a conference by its ID.
    /// </summary>
    /// <param name="id">The conference ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conference if found; otherwise, null.</returns>
    Task<Conference?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing conference in the repository.
    /// </summary>
    /// <param name="conference">The conference to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(Conference conference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists conferences with optional search and pagination.
    /// Only returns conferences that have not ended (EndDate >= today).
    /// </summary>
    /// <param name="searchQuery">Optional search query to filter by title, city, or country.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A tuple containing the list of conferences and the total count.</returns>
    Task<(List<Conference> Conferences, int TotalCount)> ListConferencesAsync(
        string? searchQuery,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
