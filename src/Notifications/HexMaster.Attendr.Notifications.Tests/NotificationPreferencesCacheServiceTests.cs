using HexMaster.Attendr.Notifications.Abstractions.Repositories;
using HexMaster.Attendr.Notifications.DomainModels;
using HexMaster.Attendr.Notifications.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Notifications.Tests;

public sealed class NotificationPreferencesCacheServiceTests
{
    [Fact]
    public async Task GetOrFetchPreferencesAsync_CachesRepositoryValue()
    {
        var profileId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            ProfileId = profileId,
            TypeChannelPreferences = new Dictionary<string, Dictionary<Abstractions.Enums.NotificationChannel, bool>>(),
            CreatedAt = DateTime.UtcNow
        };

        var repository = new Mock<INotificationPreferencesRepository>();
        repository
            .Setup(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(preferences);

        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new NotificationPreferencesCacheService(repository.Object, cache, NullLogger<NotificationPreferencesCacheService>.Instance);

        var first = await service.GetOrFetchPreferencesAsync(profileId);
        var second = await service.GetOrFetchPreferencesAsync(profileId);

        Assert.Same(preferences, first);
        Assert.Same(first, second);
        repository.Verify(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClearCache_RemovesEntry()
    {
        var profileId = Guid.NewGuid();
        var preferences = new NotificationPreferences
        {
            ProfileId = profileId,
            TypeChannelPreferences = new Dictionary<string, Dictionary<Abstractions.Enums.NotificationChannel, bool>>(),
            CreatedAt = DateTime.UtcNow
        };

        var repository = new Mock<INotificationPreferencesRepository>();
        repository
            .SetupSequence(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(preferences)
            .ReturnsAsync(preferences);

        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new NotificationPreferencesCacheService(repository.Object, cache, NullLogger<NotificationPreferencesCacheService>.Instance);

        await service.GetOrFetchPreferencesAsync(profileId);
        service.ClearCache(profileId);
        await service.GetOrFetchPreferencesAsync(profileId);

        repository.Verify(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
