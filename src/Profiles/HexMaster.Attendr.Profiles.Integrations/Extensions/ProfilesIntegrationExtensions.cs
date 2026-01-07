using System.Security.Claims;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;

namespace HexMaster.Attendr.Profiles.Integrations.Extensions;

/// <summary>
/// Extension methods for profile resolution from authenticated users.
/// </summary>
public static class ProfilesIntegrationExtensions
{
    /// <summary>
    /// Resolves a user profile from the authenticated ClaimsPrincipal.
    /// Extracts the subject ID from the JWT token claims and retrieves the corresponding profile.
    /// </summary>
    /// <param name="profilesIntegration">The profiles integration service.</param>
    /// <param name="user">The authenticated ClaimsPrincipal containing the JWT token claims.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resolved profile with ProfileId and DisplayName.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the subject ID cannot be extracted from the claims.</exception>
    /// <exception cref="ProfileNotFoundException">Thrown when no profile exists for the authenticated user.</exception>
    public static async Task<ResolveProfileResult> GetProfileFromUser(
        this Services.IProfilesIntegrationService profilesIntegration,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        var subjectId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(subjectId))
        {
            throw new UnauthorizedException("Subject ID not found in authentication token.");
        }

        var profile = await profilesIntegration.ResolveProfile(subjectId, cancellationToken);
        if (profile is null)
        {
            throw new ProfileNotFoundException();
        }

        return profile;
    }
}
