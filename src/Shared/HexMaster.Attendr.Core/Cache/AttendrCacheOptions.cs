namespace HexMaster.Attendr.Core.Cache;

public sealed class AttendrCacheOptions
{
    public const string SectionName = "Attendr:Cache";

    /// <summary>
    /// The name of the Dapr state store component to use for caching.
    /// </summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>
    /// Default TTL for cache entries, in seconds.
    /// </summary>
    public int DefaultTtlSeconds { get; set; } = 300;
}
