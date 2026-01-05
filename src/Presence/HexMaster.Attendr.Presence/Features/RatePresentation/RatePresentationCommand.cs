using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Presence.Abstractions.Dtos;

namespace HexMaster.Attendr.Presence.Features.RatePresentation;

/// <summary>
/// Command to rate a presentation and optionally mark it as a favorite.
/// </summary>
/// <param name="ProfileId">The unique identifier of the profile.</param>
/// <param name="ConferenceId">The unique identifier of the conference.</param>
/// <param name="PresentationId">The unique identifier of the presentation.</param>
/// <param name="RatingDto">The rating data containing rating value and favorite flag.</param>
public sealed record RatePresentationCommand(
    Guid ProfileId,
    Guid ConferenceId,
    Guid PresentationId,
    RatePresentationDto RatingDto) : IAttendrCommand;
