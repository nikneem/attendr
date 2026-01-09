using System.Text.Json.Serialization;

namespace HexMaster.Attendr.Groups.Data.Postgress.Entities;

/// <summary>
/// PostgreSQL entity representing a Group with JSONB data structure.
/// </summary>
public record GroupEntity(
    Guid Id,
    string Name,
    GroupSettingsEntity Settings,
    List<GroupMemberEntity> Members,
    List<GroupInvitationEntity> Invitations,
    List<GroupJoinRequestEntity> JoinRequests,
    List<FollowedConferenceEntity> FollowedConferences,
    List<GroupActivityEntity> Activities
);

public record GroupSettingsEntity(
    [property: JsonPropertyName("isPublic")] bool IsPublic,
    [property: JsonPropertyName("isSearchable")] bool IsSearchable
);

public record GroupMemberEntity(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("role")] int Role
);

public record GroupInvitationEntity(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("acceptanceCode")] string AcceptanceCode,
    [property: JsonPropertyName("expirationDate")] DateTimeOffset ExpirationDate
);

public record GroupJoinRequestEntity(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("requestedAt")] DateTimeOffset RequestedAt
);

public record FollowedConferenceEntity(
    [property: JsonPropertyName("conferenceId")] Guid ConferenceId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("imageUrl")] string? ImageUrl,
    [property: JsonPropertyName("speakersCount")] int SpeakersCount,
    [property: JsonPropertyName("sessionsCount")] int SessionsCount,
    [property: JsonPropertyName("startDate")] DateOnly StartDate,
    [property: JsonPropertyName("endDate")] DateOnly EndDate
);

public record GroupActivityEntity(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("profileId")] Guid ProfileId,
    [property: JsonPropertyName("createdAt")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("activityTypeId")] int ActivityTypeId
);
