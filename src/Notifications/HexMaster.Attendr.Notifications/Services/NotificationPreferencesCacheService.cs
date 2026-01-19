using HexMaster.Attendr.Notifications.Abstractions.DomainModels;
using HexMaster.Attendr.Notifications.Abstractions.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Notifications.Services;

/// <summary>
/// In-memory cache service for notification preferences to reduce database lookups.
/// </summary>
public interface INotificationPreferencesCacheService
{
    /// <summary>
    /// Gets notification preferences for a profile from cache or repository.
    /// </summary>
    Task<INotificationPreferences?> GetOrFetchPreferencesAsync(Guid profileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears cached preferences for a specific profile.
    /// </summary>
    void ClearCache(Guid profileId);

    /// <summary>
    /// Clears all cached preferences.
    /// </summary>
    void ClearAllCache();
}

/// <summary>
/// Implementation of notification preferences caching service.
/// Uses IMemoryCache for fast in-memory lookups.
/// </summary>
public sealed class NotificationPreferencesCacheService : INotificationPreferencesCacheService
{
    private readonly INotificationPreferencesRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<NotificationPreferencesCacheService> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public NotificationPreferencesCacheService(
        INotificationPreferencesRepository repository,
        IMemoryCache cache,
        ILogger<NotificationPreferencesCacheService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<INotificationPreferences?> GetOrFetchPreferencesAsync(
        Guid profileId, 
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(profileId);

        // Try to get from cache first
        if (_cache.TryGetValue<INotificationPreferences>(cacheKey, out var cachedPreferences))
        {
            _logger.LogDebug("Retrieved notification preferences for profile {ProfileId} from cache", profileId);
            return cachedPreferences;
        }

        // Not in cache, fetch from repository
        _logger.LogDebug("Fetching notification preferences for profile {ProfileId} from repository", profileId);
        var preferences = await _repository.GetByProfileIdAsync(profileId, cancellationToken);

        // Cache the result (even if null to avoid repeated lookups)
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            SlidingExpiration = TimeSpan.FromMinutes(5)
        };

        _cache.Set(cacheKey, preferences, cacheOptions);

        _logger.LogInformation(
            "Cached notification preferences for profile {ProfileId} (found: {Found})",
            profileId, preferences != null);

        return preferences;
    }

    public void ClearCache(Guid profileId)
    {
        var cacheKey = GetCacheKey(profileId);
        _cache.Remove(cacheKey);
        _logger.LogInformation("Cleared notification preferences cache for profile {ProfileId}", profileId);
    }

    public void ClearAllCache()
    {
        // Note: IMemoryCache doesn't have a built-in way to clear all entries
        // In production, you might want to use a more sophisticated caching solution
        _logger.LogWarning("ClearAllCache called - IMemoryCache doesn't support bulk clear. Consider using a different cache implementation if needed.");
    }

    private static string GetCacheKey(Guid profileId) => $"notif-prefs:{profileId}";
}
