using System.Text.Json.Serialization;

namespace HexMaster.Attendr.Groups.Data.Postgress.Entities;

/// <summary>
/// PostgreSQL entity for Group Check-ins with JSONB structure.
/// </summary>
public sealed record CheckInEntity(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("groupId")] Guid GroupId,
    [property: JsonPropertyName("conferenceId")] Guid ConferenceId,
    [property: JsonPropertyName("presentationId")] Guid PresentationId,
    [property: JsonPropertyName("presentationData")] PresentationDataEntity PresentationData,
    [property: JsonPropertyName("memberData")] List<CheckedInMemberEntity> MemberData,
    [property: JsonPropertyName("expiration")] DateTimeOffset Expiration
);

/// <summary>
/// Embedded presentation data in check-in.
/// </summary>
public sealed record PresentationDataEntity(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("abstract")] string Abstract,
    [property: JsonPropertyName("room")] string Room,
    [property: JsonPropertyName("startDateTime")] DateTimeOffset StartDateTime,
    [property: JsonPropertyName("endDateTime")] DateTimeOffset EndDateTime,
    [property: JsonPropertyName("speakers")] List<PresentationSpeakerEntity> Speakers
);

/// <summary>
/// Embedded speaker data in presentation.
/// </summary>
public sealed record PresentationSpeakerEntity(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("profilePictureUrl")] string? ProfilePictureUrl
);

/// <summary>
/// Embedded member data for checked-in members.
/// </summary>
public sealed record CheckedInMemberEntity(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("profilePictureUrl")] string? ProfilePictureUrl
);
