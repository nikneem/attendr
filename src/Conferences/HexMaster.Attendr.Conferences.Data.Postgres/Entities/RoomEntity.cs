namespace HexMaster.Attendr.Conferences.Data.Postgres.Entities;

/// <summary>
/// PostgreSQL entity representing a Room at a conference.
/// </summary>
public sealed class RoomEntity
{
    public Guid Id { get; set; }
    public Guid ConferenceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? ExternalId { get; set; }
}
