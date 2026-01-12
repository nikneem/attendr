using Microsoft.AspNetCore.Authorization;

namespace HexMaster.Attendr.Conferences.Api.Authorization;

/// <summary>
/// Authorization handler that validates if a user has the required permission.
/// Permissions are expected to be in the "permissions" claim as an array.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Get all permission claims (Auth0 typically uses "permissions" claim)
        var permissionsClaim = context.User.FindFirst(c => c.Type == "permissions");

        if (permissionsClaim != null)
        {
            // Auth0 sends permissions as space-separated string
            var permissions = permissionsClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
        else
        {
            // Also check for individual permission claims (alternative format)
            var hasPermission = context.User.HasClaim(c =>
                c.Type == "permissions" && c.Value == requirement.Permission);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
