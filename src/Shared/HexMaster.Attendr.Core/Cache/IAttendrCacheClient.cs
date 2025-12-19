using System.Diagnostics.CodeAnalysis;

namespace HexMaster.Attendr.Core.Cache;

public interface IAttendrCacheClient
{
    /// <summary>
    /// Cache-aside retrieval for any value type.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="factory">Factory to produce the value on cache miss.</param>
    /// <param name="ttl">Optional absolute TTL. If not provided, the configured default TTL is used.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or freshly computed value.</returns>
    Task<T?> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T?>> factory, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Explicitly set a value in cache.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="value">Value to store.</param>
    /// <param name="ttl">Optional absolute TTL. If not provided, the configured default TTL is used.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default);
}
