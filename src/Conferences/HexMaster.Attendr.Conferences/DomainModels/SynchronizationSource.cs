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
    public string? SourceUrl { get; private set; }

    /// <summary>
    /// Gets the API key for authenticating with the synchronization source.
    /// </summary>
    public string? ApiKey { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SynchronizationSource"/> class.
    /// </summary>
    private SynchronizationSource()
    {
        SourceType = SynchronizationSourceType.Sessionize;
    }

    private SynchronizationSource(SynchronizationSourceType sourceType, string? sourceUrl, string? apiKey)
    {
        SourceType = sourceType;
        SourceUrl = sourceUrl;
        ApiKey = apiKey;
    }

    /// <summary>
    /// Creates a new instance of <see cref="SynchronizationSource"/> with a source URL.
    /// </summary>
    /// <param name="sourceType">The type of synchronization source.</param>
    /// <param name="sourceUrl">The source URL.</param>
    /// <returns>A new instance of <see cref="SynchronizationSource"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when sourceUrl is null or whitespace.</exception>
    public static SynchronizationSource CreateWithUrl(SynchronizationSourceType sourceType, string sourceUrl)
    {
        if (string.IsNullOrWhiteSpace(sourceUrl))
        {
            throw new ArgumentException("Source URL cannot be empty.", nameof(sourceUrl));
        }

        return new SynchronizationSource(sourceType, sourceUrl, null);
    }

    /// <summary>
    /// Creates a new instance of <see cref="SynchronizationSource"/> with an API key.
    /// </summary>
    /// <param name="sourceType">The type of synchronization source.</param>
    /// <param name="apiKey">The API key.</param>
    /// <returns>A new instance of <see cref="SynchronizationSource"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when apiKey is null or whitespace.</exception>
    public static SynchronizationSource CreateWithApiKey(SynchronizationSourceType sourceType, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("API key cannot be empty.", nameof(apiKey));
        }

        return new SynchronizationSource(sourceType, null, apiKey);
    }

    /// <summary>
    /// Factory method to create a synchronization source from persisted data.
    /// </summary>
    /// <param name="sourceType">The type of synchronization source.</param>
    /// <param name="sourceUrl">The source URL (can be null).</param>
    /// <param name="apiKey">The API key (can be null).</param>
    /// <returns>A new instance of <see cref="SynchronizationSource"/>.</returns>
    public static SynchronizationSource FromPersisted(SynchronizationSourceType sourceType, string? sourceUrl, string? apiKey)
    {
        return new SynchronizationSource(sourceType, sourceUrl, apiKey);
    }

    /// <summary>
    /// Updates the source URL.
    /// </summary>
    /// <param name="sourceUrl">The new source URL.</param>
    /// <exception cref="ArgumentException">Thrown when sourceUrl is null or whitespace.</exception>
    public void UpdateSourceUrl(string sourceUrl)
    {
        if (string.IsNullOrWhiteSpace(sourceUrl))
        {
            throw new ArgumentException("Source URL cannot be empty.", nameof(sourceUrl));
        }

        SourceUrl = sourceUrl;
        ApiKey = null;
    }

    /// <summary>
    /// Updates the API key.
    /// </summary>
    /// <param name="apiKey">The new API key.</param>
    /// <exception cref="ArgumentException">Thrown when apiKey is null or whitespace.</exception>
    public void UpdateApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("API key cannot be empty.", nameof(apiKey));
        }

        ApiKey = apiKey;
        SourceUrl = null;
    }
}
