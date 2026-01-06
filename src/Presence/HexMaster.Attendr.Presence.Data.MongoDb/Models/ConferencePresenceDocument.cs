using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HexMaster.Attendr.Presence.Data.MongoDb.Models;

public sealed class ConferencePresenceDocument
{
    [BsonId]
    public string Id { get; set; } = string.Empty; // profileId:conferenceId

    [BsonElement("profileId")]
    [BsonRepresentation(BsonType.String)]
    public Guid ProfileId { get; set; }

    [BsonElement("conferenceId")]
    [BsonRepresentation(BsonType.String)]
    public Guid ConferenceId { get; set; }

    [BsonElement("conferenceName")]
    public string ConferenceName { get; set; } = string.Empty;

    [BsonElement("location")]
    public string Location { get; set; } = string.Empty;

    [BsonElement("startDate")]
    public DateOnly StartDate { get; set; }

    [BsonElement("endDate")]
    public DateOnly EndDate { get; set; }

    [BsonElement("isFollowing")]
    public bool IsFollowing { get; set; }

    [BsonElement("isAttending")]
    public bool IsAttending { get; set; }
}
