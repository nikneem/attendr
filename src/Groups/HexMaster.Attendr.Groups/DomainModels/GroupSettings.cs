namespace HexMaster.Attendr.Groups.DomainModels;

/// <summary>
/// Value object representing settings for a group.
/// Follows Domain-Driven Design principles with immutable initialization
/// and private setters to enforce invariants.
/// </summary>
public sealed class GroupSettings
{
    /// <summary>
    /// Gets a value indicating whether the group is public and visible to all users.
    /// </summary>
    public bool IsPublic { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the group is searchable by other attendees.
    /// </summary>
    public bool IsSearchable { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupSettings"/> class with default settings.
    /// </summary>
    private GroupSettings()
    {
        IsPublic = false;
        IsSearchable = false;
    }

    /// <summary>
    /// Creates a new instance of <see cref="GroupSettings"/> with the specified values.
    /// </summary>
    /// <param name="isPublic">Whether the group is public.</param>
    /// <param name="isSearchable">Whether the group is searchable.</param>
    /// <returns>A new instance of <see cref="GroupSettings"/>.</returns>
    public static GroupSettings Create(bool isPublic, bool isSearchable)
    {
        return new GroupSettings
        {
            IsPublic = isPublic,
            IsSearchable = isSearchable
        };
    }

    /// <summary>
    /// Creates a new instance with default values (private and not searchable).
    /// </summary>
    /// <returns>A new instance of <see cref="GroupSettings"/>.</returns>
    public static GroupSettings CreateDefault() => new();
}
