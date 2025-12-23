using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HexMaster.Attendr.Groups.Data.MongoDb.Models;

/// <summary>
/// MongoDB document representation of a Group aggregate.
/// </summary>
internal sealed class GroupDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("settings")]
    public GroupSettingsDocument Settings { get; set; } = new();

    [BsonElement("members")]
    public List<GroupMemberDocument> Members { get; set; } = new();

    [BsonElement("invitations")]
    public List<GroupInvitationDocument> Invitations { get; set; } = new();

    [BsonElement("joinRequests")]
    public List<GroupJoinRequestDocument> JoinRequests { get; set; } = new();

    [BsonElement("followedConferences")]
    public List<FollowedConferenceDocument> FollowedConferences { get; set; } = new();
}

internal sealed class GroupSettingsDocument
{
    [BsonElement("isPublic")]
    public bool IsPublic { get; set; }

    [BsonElement("isSearchable")]
    public bool IsSearchable { get; set; }
}

internal sealed class GroupMemberDocument
{
    [BsonElement("id")]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("role")]
    public int Role { get; set; }
}

internal sealed class GroupInvitationDocument
{
    [BsonElement("id")]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("acceptanceCode")]
    public string AcceptanceCode { get; set; } = string.Empty;

    [BsonElement("expirationDate")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTimeOffset ExpirationDate { get; set; }
}

internal sealed class GroupJoinRequestDocument
{
    [BsonElement("id")]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("requestedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTimeOffset RequestedAt { get; set; }
}

internal sealed class FollowedConferenceDocument
{
    [BsonElement("conferenceId")]
    [BsonRepresentation(BsonType.String)]
    public Guid ConferenceId { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("city")]
    public string City { get; set; } = string.Empty;

    [BsonElement("country")]
    public string Country { get; set; } = string.Empty;

    [BsonElement("startDate")]
    public DateOnly StartDate { get; set; }

    [BsonElement("endDate")]
    public DateOnly EndDate { get; set; }
}
