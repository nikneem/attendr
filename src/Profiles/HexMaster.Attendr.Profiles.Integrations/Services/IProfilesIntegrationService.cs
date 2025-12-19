using HexMaster.Attendr.Profiles.Abstractions.Dtos;

namespace HexMaster.Attendr.Profiles.Integrations.Services;

public interface IProfilesIntegrationService
{
    /// <summary>
    /// Resolve a profile by its subject identifier using cache-aside.
    /// Returns null when the profile does not exist.
    /// </summary>
    Task<ResolveProfileResult?> ResolveProfile(string subjectId, CancellationToken cancellationToken = default);
}
