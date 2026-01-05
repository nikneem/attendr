namespace HexMaster.Attendr.Presence.Abstractions.Dtos;

/// <summary>
/// DTO for rating and favoriting a presentation.
/// </summary>
/// <param name="Rating">The rating value (0-5), or null if no rating.</param>
/// <param name="IsFavorite">Whether the presentation is marked as favorite.</param>
public record RatePresentationDto(
    byte? Rating,
    bool IsFavorite
);
