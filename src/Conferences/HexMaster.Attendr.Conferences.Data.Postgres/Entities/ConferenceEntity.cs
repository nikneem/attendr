namespace HexMaster.Attendr.Conferences.Data.Postgres.Entities;

/// <summary>
/// PostgreSQL entity representing a Conference.
/// </summary>
public sealed class ConferenceEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsVisible { get; set; }

    // Synchronization source
    public int? SyncSourceType { get; set; }
    public string? SyncSourceLocationOrApiKey { get; set; }
}
