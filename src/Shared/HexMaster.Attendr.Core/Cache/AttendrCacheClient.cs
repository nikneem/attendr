using System.Text.Json;
using StackExchange.Redis;

namespace HexMaster.Attendr.Core.Cache;

public sealed class AttendrCacheClient : IAttendrCacheClient
{
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly TimeSpan _defaultTtl;
    private readonly JsonSerializerOptions _jsonOptions;

    public AttendrCacheClient(IConnectionMultiplexer multiplexer, AttendrCacheOptions options)
    {
        _multiplexer = multiplexer ?? throw new ArgumentNullException(nameof(multiplexer));
        ArgumentNullException.ThrowIfNull(options);
        _defaultTtl = TimeSpan.FromSeconds(Math.Max(1, options.DefaultTtlSeconds));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T?>> factory, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
        ArgumentNullException.ThrowIfNull(factory);

        var db = _multiplexer.GetDatabase();
        var cached = await db.StringGetAsync(key).ConfigureAwait(false);
        if (cached.HasValue)
        {
            try
            {
                var value = JsonSerializer.Deserialize<T>(cached.ToString(), _jsonOptions);
                return value;
            }
            catch
            {
                // If deserialization fails, fall through to recompute and overwrite
            }
        }

        var fresh = await factory(cancellationToken).ConfigureAwait(false);
        if (fresh is null)
        {
            return default;
        }

        var payload = JsonSerializer.Serialize(fresh, _jsonOptions);
        var expiry = ttl ?? _defaultTtl;
        await db.StringSetAsync(key, payload, expiry).ConfigureAwait(false);
        return fresh;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
        var db = _multiplexer.GetDatabase();
        var payload = JsonSerializer.Serialize(value, _jsonOptions);
        var expiry = ttl ?? _defaultTtl;
        await db.StringSetAsync(key, payload, expiry).ConfigureAwait(false);
    }
}
