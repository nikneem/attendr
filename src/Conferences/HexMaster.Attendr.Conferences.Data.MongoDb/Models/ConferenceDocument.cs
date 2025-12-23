using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HexMaster.Attendr.Conferences.Data.MongoDb.Models;

/// <summary>
/// MongoDB document representation of a Conference aggregate.
/// </summary>
internal sealed class ConferenceDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("city")]
    public string City { get; set; } = string.Empty;

    [BsonElement("country")]
    public string Country { get; set; } = string.Empty;

    [BsonElement("startDate")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime StartDate { get; set; }

    [BsonElement("endDate")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime EndDate { get; set; }

    [BsonElement("imageUrl")]
    [BsonIgnoreIfNull]
    public string? ImageUrl { get; set; }

    [BsonElement("synchronizationSource")]
    [BsonIgnoreIfNull]
    public SynchronizationSourceDocument? SynchronizationSource { get; set; }

    [BsonElement("rooms")]
    public List<RoomDocument> Rooms { get; set; } = new();

    [BsonElement("speakers")]
    public List<SpeakerDocument> Speakers { get; set; } = new();

    [BsonElement("presentations")]
    public List<PresentationDocument> Presentations { get; set; } = new();
}

internal sealed class SynchronizationSourceDocument
{
    [BsonElement("sourceType")]
    public int SourceType { get; set; }

    [BsonElement("sourceUrl")]
    [BsonIgnoreIfNull]
    public string? SourceLocationOrApiKey { get; set; }

}

internal sealed class RoomDocument
{
    [BsonElement("id")]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("externalId")]
    [BsonIgnoreIfNull]
    public string? ExternalId { get; set; }

    [BsonElement("capacity")]
    public int Capacity { get; set; }
}

internal sealed class SpeakerDocument
{
    [BsonElement("id")]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("externalId")]
    [BsonIgnoreIfNull]
    public string? ExternalId { get; set; }

    [BsonElement("company")]
    [BsonIgnoreIfNull]
    public string? Company { get; set; }

    [BsonElement("profilePictureUrl")]
    [BsonIgnoreIfNull]
    public string? ProfilePictureUrl { get; set; }
}

internal sealed class PresentationDocument
{
    [BsonElement("id")]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("abstract")]
    public string Abstract { get; set; } = string.Empty;

    [BsonElement("startDateTime")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime StartDateTime { get; set; }

    [BsonElement("endDateTime")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime EndDateTime { get; set; }

    [BsonElement("roomId")]
    [BsonRepresentation(BsonType.String)]
    public Guid RoomId { get; set; }

    [BsonElement("externalId")]
    [BsonIgnoreIfNull]
    public string? ExternalId { get; set; }

    [BsonElement("speakerIds")]
    [BsonRepresentation(BsonType.String)]
    public List<Guid> SpeakerIds { get; set; } = new();
}
