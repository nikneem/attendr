namespace HexMaster.Attendr.Core.Cache;

public sealed class AttendrCacheOptions
{
    public const string SectionName = "Attendr:Cache";

    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Default TTL for cache entries, in seconds.
    /// </summary>
    public int DefaultTtlSeconds { get; set; } = 300;
}
