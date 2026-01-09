namespace HexMaster.Attendr.Conferences.Data.Postgres.Entities;

/// <summary>
/// PostgreSQL junction table entity representing the many-to-many relationship between Presentations and Speakers.
/// </summary>
public sealed class PresentationSpeakerEntity
{
    public Guid PresentationId { get; set; }
    public Guid SpeakerId { get; set; }
}
