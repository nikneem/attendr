using Microsoft.AspNetCore.Authorization;

namespace HexMaster.Attendr.Conferences.Api.Authorization;

/// <summary>
/// Requirement that checks if the user has a specific permission in their claims.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }
}
