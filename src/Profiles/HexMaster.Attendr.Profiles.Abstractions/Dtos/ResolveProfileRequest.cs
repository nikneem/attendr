namespace HexMaster.Attendr.Profiles.Abstractions.Dtos;

/// <summary>
/// Request DTO to resolve a profile by its subject identifier.
/// </summary>
/// <param name="SubjectId">The subject id from the identity provider.</param>
public sealed record ResolveProfileRequest(string SubjectId);
