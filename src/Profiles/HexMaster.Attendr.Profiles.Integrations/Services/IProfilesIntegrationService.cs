using HexMaster.Attendr.Profiles.Abstractions.Dtos;

namespace HexMaster.Attendr.Profiles.Integrations.Services;

public interface IProfilesIntegrationService
{
    /// <summary>
    /// Resolve a profile by its subject identifier using cache-aside.
    /// Returns null when the profile does not exist.
    /// </summary>
    Task<ResolveProfileResult?> ResolveProfile(string subjectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets profile details by profile ID using cache-aside pattern.
    /// First checks cache, then fetches from API if not found and stores in cache.
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Profile details if found; otherwise, null.</returns>
    Task<ProfileDetailsDto?> GetProfileDetails(string profileId, CancellationToken cancellationToken = default);
}
