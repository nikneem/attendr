using HexMaster.Attendr.Conferences.Api.Authorization;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Services;
using System.Security.Claims;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

internal static class EndpointHelpers
{
    internal static async Task<(Guid? profileId, bool isAdmin)> ResolveProfileAsync(
        IProfilesIntegrationService profilesIntegration,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var isAdmin = IsAdminUser(user);
        Guid? profileId = null;
        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
            if (Guid.TryParse(profile.ProfileId, out var id))
                profileId = id;
        }
        catch (UnauthorizedException) { }
        catch (ProfileNotFoundException) { }
        return (profileId, isAdmin);
    }

    internal static bool IsAdminUser(ClaimsPrincipal user)
    {
        var permissionsClaim = user.FindFirst("permissions");
        if (permissionsClaim == null) return false;
        var permissions = permissionsClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return permissions.Contains(Permissions.AdminAttendr);
    }
}
