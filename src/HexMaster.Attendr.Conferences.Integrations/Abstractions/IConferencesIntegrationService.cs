using HexMaster.Attendr.Conferences.Abstractions.Dtos;

namespace HexMaster.Attendr.Conferences.Integrations.Abstractions;

/// <summary>
/// Service for integrating with the Conferences API.
/// Provides methods to fetch conference data with caching.
/// </summary>
public interface IConferencesIntegrationService
{
    /// <summary>
    /// Gets conference details by ID using cache-aside pattern.
    /// First checks cache, then fetches from API if not found and stores in cache.
    /// </summary>
    /// <param name="id">The conference ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Conference details if found; otherwise, null.</returns>
    Task<ConferenceDetailsDto?> GetConferenceDetails(Guid id, CancellationToken cancellationToken = default);
}
