namespace HexMaster.Attendr.Presence.Abstractions.Dtos;

/// <summary>
/// DTO containing conference attendance information for a profile.
/// </summary>
/// <param name="ConferenceId">The unique identifier of the conference.</param>
/// <param name="IsFollowing">Indicates whether the profile is following this conference.</param>
/// <param name="IsAttending">Indicates whether the profile is attending this conference.</param>
/// <param name="FavoritePresentationIds">List of presentation IDs marked as favorites.</param>
public sealed record ConferenceAttendanceDto(
    Guid ConferenceId,
    bool IsFollowing,
    bool IsAttending,
    IReadOnlyCollection<Guid> FavoritePresentationIds);
