using HexMaster.Attendr.Core.DomainModels;

namespace HexMaster.Attendr.Conferences.DomainModels;

/// <summary>
/// Represents a topic that can be associated with presentations.
/// </summary>
public sealed class Topic : DomainModel<Guid>
{
    /// <summary>
    /// Gets the unique key of the topic.
    /// </summary>
    public string Key { get; private set; }

    /// <summary>
    /// Gets the display name of the topic.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this topic is visible to users.
    /// </summary>
    public bool IsVisible { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Topic"/> class.
    /// </summary>
    private Topic(
        Guid id,
        string key,
        string name,
        bool isVisible,
        DateTimeOffset createdOn)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Topic key cannot be empty.", nameof(key));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Topic name cannot be empty.", nameof(name));
        }

        Key = key;
        Name = name;
        IsVisible = isVisible;
        SetCreatedOn(createdOn);
    }

    /// <summary>
    /// Factory method to create a new topic via the automated AI system (hidden by default).
    /// </summary>
    /// <param name="key">The unique key of the topic.</param>
    /// <param name="name">The display name of the topic.</param>
    /// <returns>A new instance of <see cref="Topic"/> that is hidden by default.</returns>
    public static Topic Create(string key, string name)
    {
        var id = Guid.NewGuid();
        return new Topic(id, key, name, isVisible: false, DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Factory method to create a new topic manually (visible by default).
    /// </summary>
    /// <param name="key">The unique key of the topic.</param>
    /// <param name="name">The display name of the topic.</param>
    /// <returns>A new instance of <see cref="Topic"/> that is visible by default.</returns>
    public static Topic CreateManually(string key, string name)
    {
        var id = Guid.NewGuid();
        return new Topic(id, key, name, isVisible: true, DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Factory method to load a topic from persisted data.
    /// </summary>
    /// <param name="id">The unique identifier of the topic.</param>
    /// <param name="key">The unique key of the topic.</param>
    /// <param name="name">The display name of the topic.</param>
    /// <param name="isVisible">Whether the topic is visible.</param>
    /// <param name="createdOn">The creation date and time.</param>
    /// <returns>A topic instance loaded from persistence.</returns>
    public static Topic FromPersisted(
        Guid id,
        string key,
        string name,
        bool isVisible,
        DateTimeOffset createdOn)
    {
        return new Topic(id, key, name, isVisible, createdOn);
    }

    /// <summary>
    /// Makes the topic visible to users.
    /// </summary>
    public void MakeVisible()
    {
        IsVisible = true;
    }

    /// <summary>
    /// Hides the topic from users.
    /// </summary>
    public void Hide()
    {
        IsVisible = false;
    }

    /// <summary>
    /// Updates the topic's key and name.
    /// </summary>
    /// <param name="key">The new unique key of the topic.</param>
    /// <param name="name">The new display name of the topic.</param>
    public void UpdateDetails(string key, string name)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Topic key cannot be empty.", nameof(key));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Topic name cannot be empty.", nameof(name));
        }

        Key = key;
        Name = name;
    }
}
