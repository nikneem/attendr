namespace HexMaster.Attendr.Conferences.Api.Authorization;

/// <summary>
/// Constants for authorization policy names.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Policy that requires the user to be authenticated (default).
    /// </summary>
    public const string Authenticated = "Authenticated";

    /// <summary>
    /// Policy that requires the user to have admin permissions.
    /// </summary>
    public const string Admin = "Admin";
}
