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
    [property: JsonPropertyName("startDateTime")] DateTimeOffset StartDateTime,
    [property: JsonPropertyName("endDateTime")] DateTimeOffset EndDateTime,
    [property: JsonPropertyName("topics")] List<PresentationTopicEmbedded> Topics,
    [property: JsonPropertyName("speakers")] List<SpeakerEmbedded> Speakers,
    [property: JsonPropertyName("isRated")] bool IsRated,
    [property: JsonPropertyName("isFavorite")] bool IsFavorite,
    [property: JsonPropertyName("isCheckedIn")] bool IsCheckedIn,
    [property: JsonPropertyName("checkedInAt")] DateTimeOffset? CheckedInAt,
    [property: JsonPropertyName("rating")] byte? Rating,
    [property: JsonPropertyName("isRecommended")] bool IsRecommended,
    [property: JsonPropertyName("isPreferred")] bool IsPreferred
);

public sealed record PresentationTopicEmbedded(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("name")] string Name
);
