namespace HexMaster.Attendr.Conferences.Data.Postgres.Entities;

/// <summary>
/// PostgreSQL entity representing a Presentation at a conference.
/// </summary>
public sealed class PresentationEntity
{
    public Guid Id { get; set; }
    public Guid ConferenceId { get; set; }
    public Guid RoomId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public DateTimeOffset StartDateTime { get; set; }
    public DateTimeOffset EndDateTime { get; set; }
    public bool IsAnalysed { get; set; }
    public string? ExternalId { get; set; }
}
