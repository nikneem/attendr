using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Presence.Features.ResetConferenceRatings;

/// <summary>
/// Command to reset ratings for all presentations of a conference.
/// Sets IsRated to false, IsFavorite to false, and Rating to null for all presentations.
/// </summary>
/// <param name="ProfileId">The unique identifier of the profile.</param>
/// <param name="ConferenceId">The unique identifier of the conference.</param>
public sealed record ResetConferenceRatingsCommand(
    Guid ProfileId,
    Guid ConferenceId) : IAttendrCommand;
