namespace HexMaster.Attendr.Groups.DomainModels;

/// <summary>
/// Represents an activity recorded in a group.
/// Activities provide a log of group actions for members to see.
/// </summary>
public sealed class GroupActivity
{
    /// <summary>
    /// Gets the unique identifier for the activity.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the unique identifier of the profile that triggered this activity.
    /// </summary>
    public Guid ProfileId { get; }

    /// <summary>
    /// Gets the timestamp when this activity was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the description of the activity.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupActivity"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the activity.</param>
    /// <param name="profileId">The unique identifier of the profile that triggered this activity.</param>
    /// <param name="createdAt">The timestamp when this activity was created.</param>
    /// <param name="description">The description of the activity.</param>
    /// <exception cref="ArgumentException">Thrown when id or profileId is empty, or description is null or whitespace.</exception>
    public GroupActivity(Guid id, Guid profileId, DateTimeOffset createdAt, string description)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Activity ID cannot be empty.", nameof(id));
        }

        if (profileId == Guid.Empty)
        {
            throw new ArgumentException("Profile ID cannot be empty.", nameof(profileId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));

        Id = id;
        ProfileId = profileId;
        CreatedAt = createdAt;
        Description = description.Trim();
    }
}
