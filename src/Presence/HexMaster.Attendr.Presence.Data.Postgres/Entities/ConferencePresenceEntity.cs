using System.Text.Json.Serialization;

namespace HexMaster.Attendr.Presence.Data.Postgres.Entities;

/// <summary>
/// PostgreSQL entity for ConferencePresence with JSONB structure.
/// </summary>
public sealed record ConferencePresenceEntity(
    [property: JsonPropertyName("profileId")] Guid ProfileId,
    [property: JsonPropertyName("conferenceId")] Guid ConferenceId,
    [property: JsonPropertyName("conferenceName")] string ConferenceName,
    [property: JsonPropertyName("location")] string Location,
    [property: JsonPropertyName("imageUrl")] string? ImageUrl,
    [property: JsonPropertyName("startDate")] DateOnly StartDate,
    [property: JsonPropertyName("endDate")] DateOnly EndDate,
    [property: JsonPropertyName("isFollowing")] bool IsFollowing,
    [property: JsonPropertyName("isAttending")] bool IsAttending,
    [property: JsonPropertyName("presentations")] List<PresentationPresenceEmbedded> Presentations
);

public sealed record PresentationPresenceEmbedded(
    [property: JsonPropertyName("presentationId")] Guid PresentationId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("abstract")] string Abstract,
    [property: JsonPropertyName("room")] string Room,
    [property: JsonPropertyName("startDateTime")] DateTime StartDateTime,
    [property: JsonPropertyName("endDateTime")] DateTime EndDateTime,
    [property: JsonPropertyName("speakers")] List<SpeakerEmbedded> Speakers,
    [property: JsonPropertyName("topics")] List<PresentationTopicEmbedded> Topics,
    [property: JsonPropertyName("isRated")] bool IsRated,
    [property: JsonPropertyName("isFavorite")] bool IsFavorite,
    [property: JsonPropertyName("isCheckedIn")] bool IsCheckedIn,
    [property: JsonPropertyName("checkedInAt")] DateTimeOffset? CheckedInAt,
    [property: JsonPropertyName("rating")] byte? Rating,
    [property: JsonPropertyName("isRecommended")] bool IsRecommended,
    [property: JsonPropertyName("isPreferred")] bool IsPreferred
);

public sealed record SpeakerEmbedded(
    [property: JsonPropertyName("speakerId")] Guid SpeakerId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("profilePictureUrl")] string? ProfilePictureUrl
);
