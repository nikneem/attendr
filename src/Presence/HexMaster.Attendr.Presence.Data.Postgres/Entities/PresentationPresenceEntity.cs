using System.Text.Json.Serialization;

namespace HexMaster.Attendr.Presence.Data.Postgres.Entities;

/// <summary>
/// PostgreSQL entity for PresentationPresence with JSONB structure.
/// </summary>
public sealed record PresentationPresenceEntity(
    [property: JsonPropertyName("profileId")] Guid ProfileId,
    [property: JsonPropertyName("conferenceId")] Guid ConferenceId,
    [property: JsonPropertyName("presentationId")] Guid PresentationId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("abstract")] string Abstract,
    [property: JsonPropertyName("room")] string Room,
    [property: JsonPropertyName("startDateTime")] DateTime StartDateTime,
    [property: JsonPropertyName("endDateTime")] DateTime EndDateTime,
    [property: JsonPropertyName("speakers")] List<SpeakerEmbedded> Speakers,
    [property: JsonPropertyName("isRated")] bool IsRated,
    [property: JsonPropertyName("isFavorite")] bool IsFavorite,
    [property: JsonPropertyName("isCheckedIn")] bool IsCheckedIn,
    [property: JsonPropertyName("checkedInAt")] DateTimeOffset? CheckedInAt,
    [property: JsonPropertyName("rating")] byte? Rating
);
