using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Presence.Features.GetConferenceWithPresentations;

/// <summary>
/// Query to retrieve conference details with presentations for a profile.
/// </summary>
/// <param name="ProfileId">The unique identifier of the profile.</param>
/// <param name="ConferenceId">The unique identifier of the conference.</param>
public sealed record GetConferenceWithPresentationsQuery(
    Guid ProfileId,
    Guid ConferenceId);
