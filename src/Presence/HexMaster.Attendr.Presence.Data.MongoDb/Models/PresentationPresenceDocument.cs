using MongoDB.Bson.Serialization.Attributes;

namespace HexMaster.Attendr.Presence.Data.MongoDb.Models;

public sealed class PresentationPresenceDocument
{
    [BsonId]
    public string Id { get; set; } = string.Empty; // profileId:conferenceId:presentationId

    [BsonElement("profileId")]
    public Guid ProfileId { get; set; }

    [BsonElement("conferenceId")]
    public Guid ConferenceId { get; set; }

    [BsonElement("presentationId")]
    public Guid PresentationId { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("abstract")]
    public string Abstract { get; set; } = string.Empty;

    [BsonElement("room")]
    public string Room { get; set; } = string.Empty;

    [BsonElement("startDateTime")]
    public DateTime StartDateTime { get; set; }

    [BsonElement("endDateTime")]
    public DateTime EndDateTime { get; set; }

    [BsonElement("isRated")]
    public bool IsRated { get; set; }

    [BsonElement("isFavorite")]
    public bool IsFavorite { get; set; }

    [BsonElement("isCheckedIn")]
    public bool IsCheckedIn { get; set; }

    [BsonElement("rating")]
    public byte? Rating { get; set; }

    [BsonElement("speakers")]
    public List<PresentationSpeakerDocument> Speakers { get; set; } = new();
}

public sealed class PresentationSpeakerDocument
{
    [BsonElement("speakerId")]
    public Guid SpeakerId { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("profilePictureUrl")]
    public string ProfilePictureUrl { get; set; } = string.Empty;
}
