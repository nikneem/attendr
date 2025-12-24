namespace HexMaster.Attendr.Groups.Abstractions.Dtos;

/// <summary>
/// Request to follow a conference.
/// </summary>
/// <param name="ConferenceId">The unique identifier of the conference to follow.</param>
public sealed record FollowConferenceRequest(Guid ConferenceId);
