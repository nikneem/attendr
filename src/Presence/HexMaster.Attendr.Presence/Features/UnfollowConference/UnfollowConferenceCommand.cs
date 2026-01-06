using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Presence.Features.UnfollowConference;

/// <summary>
/// Command to unfollow a conference by deleting the conference presence record.
/// </summary>
/// <param name="ConferenceId">The unique identifier of the conference to unfollow.</param>
/// <param name="ProfileId">The unique identifier of the profile.</param>
public sealed record UnfollowConferenceCommand(Guid ConferenceId, Guid ProfileId) : IAttendrCommand;
