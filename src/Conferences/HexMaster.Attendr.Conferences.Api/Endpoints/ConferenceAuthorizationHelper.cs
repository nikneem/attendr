using HexMaster.Attendr.Conferences.Api.Authorization;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Services;
using System.Security.Claims;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

internal static class ConferenceAuthorizationHelper
{
    internal static async Task<IResult?> AuthorizeConferenceAccessAsync(
        Guid conferenceId,
        ClaimsPrincipal user,
        IProfilesIntegrationService profilesIntegration,
        IConferenceRepository conferenceRepository,
        CancellationToken cancellationToken)
    {
        if (IsAdminUser(user)) return null;

        Guid? profileId = null;
        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
            if (Guid.TryParse(profile.ProfileId, out var pid)) profileId = pid;
        }
        catch (UnauthorizedException) { return Results.Unauthorized(); }
        catch (ProfileNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }

        if (profileId is null) return Results.Unauthorized();

        var conference = await conferenceRepository.GetByIdAsync(conferenceId, cancellationToken);
        if (conference is null) return Results.NotFound(new { error = "Conference not found" });

        if (conference.CreatedByProfileId != profileId)
            return Results.Forbid();

        return null;
    }

    internal static bool IsAdminUser(ClaimsPrincipal user)
    {
        var permissionsClaim = user.FindFirst("permissions");
        if (permissionsClaim == null) return false;
        return permissionsClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(Permissions.AdminAttendr);
    }
}
