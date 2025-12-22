using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HexMaster.Attendr.Profiles.Data.MongoDb.Models;

/// <summary>
/// MongoDB document representation of a Profile aggregate.
/// </summary>
internal sealed class ProfileDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("subjectId")]
    public string SubjectId { get; set; } = string.Empty;

    [BsonElement("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [BsonElement("firstName")]
    public string? FirstName { get; set; }

    [BsonElement("lastName")]
    public string? LastName { get; set; }

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("employee")]
    public string? Employee { get; set; }

    [BsonElement("tagLine")]
    public string? TagLine { get; set; }

    [BsonElement("isSearchable")]
    public bool IsSearchable { get; set; }

    [BsonElement("enabled")]
    public bool Enabled { get; set; }
}
