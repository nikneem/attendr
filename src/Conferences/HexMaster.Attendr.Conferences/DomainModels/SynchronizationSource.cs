namespace HexMaster.Attendr.Conferences.DomainModels;

/// <summary>
/// Value object representing the synchronization source configuration for a conference.
/// Follows Domain-Driven Design principles with immutable initialization.
/// </summary>
public sealed class SynchronizationSource
{
    /// <summary>
    /// Gets the type of synchronization source.
    /// </summary>
    public SynchronizationSourceType SourceType { get; private set; }

    /// <summary>
    /// Gets the source URL for the synchronization endpoint.
    /// </summary>
    public string? SourceLocationOrApiKey { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SynchronizationSource"/> class.
    /// </summary>
    private SynchronizationSource()
    {
        SourceType = SynchronizationSourceType.Sessionize;
    }

    private SynchronizationSource(SynchronizationSourceType sourceType, string? sourceLocationOrApiKey)
    {
        SourceType = sourceType;
        SourceLocationOrApiKey = sourceLocationOrApiKey;
    }

    /// <summary>
    /// Creates a new instance of <see cref="SynchronizationSource"/> with a source URL.
    /// </summary>
    /// <param name="sourceType">The type of synchronization source.</param>
    /// <param name="sourceUrl">The source URL.</param>
    /// <returns>A new instance of <see cref="SynchronizationSource"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when sourceUrl is null or whitespace.</exception>
    public static SynchronizationSource CreateWithUrl(SynchronizationSourceType sourceType, string sourceLocationOrApiKey)
    {
        if (string.IsNullOrWhiteSpace(sourceLocationOrApiKey))
        {
            throw new ArgumentException("Source URL cannot be empty.", nameof(sourceLocationOrApiKey));
        }

        return new SynchronizationSource(sourceType, sourceLocationOrApiKey);
    }

    /// <summary>
    /// Factory method to create a synchronization source from persisted data.
    /// </summary>
    /// <param name="sourceType">The type of synchronization source.</param>
    /// <param name="sourceUrl">The source URL (can be null).</param>
    /// <param name="apiKey">The API key (can be null).</param>
    /// <returns>A new instance of <see cref="SynchronizationSource"/>.</returns>
    public static SynchronizationSource FromPersisted(SynchronizationSourceType sourceType, string? sourceLocationOrApiKey)
    {
        return new SynchronizationSource(sourceType, sourceLocationOrApiKey);
    }

    /// <summary>
    /// Updates the source URL.
    /// </summary>
    /// <param name="sourceUrl">The new source URL.</param>
    /// <exception cref="ArgumentException">Thrown when sourceUrl is null or whitespace.</exception>
    public void UpdateSourceLocation(string sourceUrl)
    {
        if (string.IsNullOrWhiteSpace(sourceUrl))
        {
            throw new ArgumentException("Source URL cannot be empty.", nameof(sourceUrl));
        }

        SourceLocationOrApiKey = sourceUrl;
    }


}
