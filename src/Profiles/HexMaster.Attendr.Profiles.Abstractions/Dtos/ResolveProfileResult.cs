namespace HexMaster.Attendr.Profiles.Abstractions.Dtos;

/// <summary>
/// Result DTO containing resolved profile information.
/// </summary>
/// <param name="ProfileId">The profile identifier.</param>
/// <param name="DisplayName">The display name for the profile.</param>
public sealed record ResolveProfileResult(string ProfileId, string DisplayName);
