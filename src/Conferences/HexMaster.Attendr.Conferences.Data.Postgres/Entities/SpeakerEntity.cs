namespace HexMaster.Attendr.Conferences.Data.Postgres.Entities;

/// <summary>
/// PostgreSQL entity representing a Speaker at a conference.
/// </summary>
public sealed class SpeakerEntity
{
    public Guid Id { get; set; }
    public Guid ConferenceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? ExternalId { get; set; }
}
