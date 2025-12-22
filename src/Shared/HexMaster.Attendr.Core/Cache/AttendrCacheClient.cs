using Dapr.Client;

namespace HexMaster.Attendr.Core.Cache;

public sealed class AttendrCacheClient : IAttendrCacheClient
{
    private readonly DaprClient _daprClient;
    private readonly TimeSpan _defaultTtl;
    private readonly string _storeName;

    public AttendrCacheClient(DaprClient daprClient, AttendrCacheOptions options)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        ArgumentNullException.ThrowIfNull(options);
        _defaultTtl = TimeSpan.FromSeconds(Math.Max(1, options.DefaultTtlSeconds));
        _storeName = string.IsNullOrWhiteSpace(options.StoreName)
            ? throw new ArgumentException("StoreName is required", nameof(options))
            : options.StoreName;
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T?>> factory, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
        ArgumentNullException.ThrowIfNull(factory);

        try
        {
            var cached = await _daprClient.GetStateAsync<T>(_storeName, key, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (cached is not null)
            {
                return cached;
            }
        }
        catch
        {
            // If retrieval fails, fall through to recompute
        }

        var fresh = await factory(cancellationToken).ConfigureAwait(false);
        if (fresh is null)
        {
            return default;
        }

        var expiry = ttl ?? _defaultTtl;
        var metadata = new Dictionary<string, string>
        {
            { "ttlInSeconds", ((int)expiry.TotalSeconds).ToString() }
        };

        await _daprClient.SaveStateAsync(_storeName, key, fresh, metadata: metadata, cancellationToken: cancellationToken).ConfigureAwait(false);
        return fresh;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null or whitespace", nameof(key));

        var expiry = ttl ?? _defaultTtl;
        var metadata = new Dictionary<string, string>
        {
            { "ttlInSeconds", ((int)expiry.TotalSeconds).ToString() }
        };

        await _daprClient.SaveStateAsync(_storeName, key, value, metadata: metadata, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
