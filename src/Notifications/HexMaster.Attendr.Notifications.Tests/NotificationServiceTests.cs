using HexMaster.Attendr.Notifications.Abstractions.DomainModels;
using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.Abstractions.Repositories;
using HexMaster.Attendr.Notifications.Abstractions.Services;
using HexMaster.Attendr.Notifications.DomainModels;
using HexMaster.Attendr.Notifications.Models;
using HexMaster.Attendr.Notifications.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Notifications.Tests;

public sealed class NotificationServiceTests
{
    [Fact]
    public async Task CreateNotificationAsync_ThrowsForUnknownType()
    {
        var notificationRepository = new Mock<INotificationRepository>();
        var preferencesRepository = new Mock<INotificationPreferencesRepository>();
        var typeService = new Mock<INotificationTypeService>();
        typeService.Setup(s => s.GetTypeByKey("missing"))
            .Returns((Abstractions.Models.INotificationType?)null);

        var sut = new NotificationService(notificationRepository.Object, preferencesRepository.Object, typeService.Object, NullLogger<NotificationService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.CreateNotificationAsync(Guid.NewGuid(), "missing", "title", "message"));
    }

    [Fact]
    public async Task CreateNotificationAsync_StacksExistingNotification()
    {
        var profileId = Guid.NewGuid();
        var type = CreateNotificationType(allowsStacking: true);

        var existing = new Notification
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            TypeKey = type.TypeKey,
            Severity = type.Severity,
            Title = "title",
            Message = "message",
            StackKey = "stack-key",
            Count = 1,
            CreatedAt = DateTime.UtcNow.AddMinutes(-1),
            ChannelDeliveries = BuildChannelDeliveries()
        };

        var notificationRepository = new Mock<INotificationRepository>();
        notificationRepository
            .Setup(r => r.FindStackableNotificationAsync(profileId, type.TypeKey, "stack-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        Notification? updated = null;
        notificationRepository
            .Setup(r => r.UpdateAsync(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>((n, _) => updated = (Notification)n)
            .Returns(Task.CompletedTask);

        var preferencesRepository = new Mock<INotificationPreferencesRepository>();
        var typeService = new Mock<INotificationTypeService>();
        typeService.Setup(s => s.GetTypeByKey(type.TypeKey)).Returns(type);

        var sut = new NotificationService(notificationRepository.Object, preferencesRepository.Object, typeService.Object, NullLogger<NotificationService>.Instance);

        var result = await sut.CreateNotificationAsync(profileId, type.TypeKey, "title", "message", stackKey: "stack-key");

        Assert.Equal(2, updated!.Count);
        Assert.NotNull(updated.LastOccurredAt);
        Assert.Same(existing, result);
        notificationRepository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        notificationRepository.Verify(r => r.AddAsync(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateNotificationAsync_CreatesNewNotificationAndAppliesPreferences()
    {
        var profileId = Guid.NewGuid();
        var type = CreateNotificationType(allowsStacking: true);

        var preferences = new NotificationPreferences
        {
            ProfileId = profileId,
            TypeChannelPreferences = new Dictionary<string, Dictionary<NotificationChannel, bool>>
            {
                [type.TypeKey] = new()
                {
                    [NotificationChannel.InApp] = true,
                    [NotificationChannel.Email] = false,
                    [NotificationChannel.Push] = true
                }
            },
            DoNotDisturbUntil = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var notificationRepository = new Mock<INotificationRepository>();
        notificationRepository
            .Setup(r => r.FindStackableNotificationAsync(profileId, type.TypeKey, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((INotification?)null);

        Notification? added = null;
        notificationRepository
            .Setup(r => r.AddAsync(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>((n, _) => added = (Notification)n)
            .Returns(Task.CompletedTask);

        var preferencesRepository = new Mock<INotificationPreferencesRepository>();
        preferencesRepository
            .Setup(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(preferences);

        var typeService = new Mock<INotificationTypeService>();
        typeService.Setup(s => s.GetTypeByKey(type.TypeKey)).Returns(type);

        var sut = new NotificationService(notificationRepository.Object, preferencesRepository.Object, typeService.Object, NullLogger<NotificationService>.Instance);

        var result = await sut.CreateNotificationAsync(profileId, type.TypeKey, "welcome", "hello", stackKey: "stack");

        Assert.NotNull(added);
        Assert.Equal(profileId, added!.ProfileId);
        Assert.Equal(type.Severity, added.Severity);
        Assert.NotNull(added.ExpiresAt);
        Assert.InRange(added.ExpiresAt!.Value, DateTime.UtcNow.AddDays(29.5), DateTime.UtcNow.AddDays(30.5));
        Assert.Equal(3, added.ChannelDeliveries.Count);
        Assert.All(added.ChannelDeliveries.Values, c => Assert.Equal(DeliveryStatus.Skipped, c.Status));
        Assert.False(added.ChannelDeliveries[NotificationChannel.Email].Enabled);
        Assert.False(added.ChannelDeliveries[NotificationChannel.Email].Status == DeliveryStatus.Pending);
        Assert.Same(added, result);
        notificationRepository.Verify(r => r.AddAsync(added, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkMultipleAsReadAsync_MarksEveryNotification()
    {
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var notificationRepository = new Mock<INotificationRepository>(MockBehavior.Strict);
        notificationRepository
            .Setup(r => r.MarkAsReadAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var preferencesRepository = new Mock<INotificationPreferencesRepository>();
        var typeService = new Mock<INotificationTypeService>();
        typeService.Setup(s => s.GetTypeByKey(It.IsAny<string>()))
            .Returns(CreateNotificationType());

        var sut = new NotificationService(notificationRepository.Object, preferencesRepository.Object, typeService.Object, NullLogger<NotificationService>.Instance);

        await sut.MarkMultipleAsReadAsync(ids);

        foreach (var id in ids)
        {
            notificationRepository.Verify(r => r.MarkAsReadAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    private static NotificationType CreateNotificationType(bool allowsStacking = false) => new()
    {
        TypeKey = "sample.type",
        DisplayName = "Sample",
        Description = "Sample description",
        Severity = NotificationSeverity.Info,
        AllowsStacking = allowsStacking,
        StackWindowSeconds = allowsStacking ? 3600 : null,
        DefaultChannelSettings = new Dictionary<NotificationChannel, bool>
        {
            [NotificationChannel.InApp] = true,
            [NotificationChannel.Email] = true,
            [NotificationChannel.Push] = true
        },
        AvailableChannels = new Dictionary<NotificationChannel, bool>
        {
            [NotificationChannel.InApp] = true,
            [NotificationChannel.Email] = true,
            [NotificationChannel.Push] = true
        }
    };

    private static Dictionary<NotificationChannel, ChannelDeliveryInfo> BuildChannelDeliveries() => new()
    {
        [NotificationChannel.InApp] = new ChannelDeliveryInfo { Enabled = true, Status = DeliveryStatus.Pending },
        [NotificationChannel.Email] = new ChannelDeliveryInfo { Enabled = true, Status = DeliveryStatus.Pending },
        [NotificationChannel.Push] = new ChannelDeliveryInfo { Enabled = true, Status = DeliveryStatus.Pending }
    };

    [Fact]
    public async Task GetNotificationsAsync_ReturnsNotificationsFromRepository()
    {
        var profileId = Guid.NewGuid();
        var expectedNotifications = new List<INotification>
        {
            new Notification
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                TypeKey = "test.type",
                Severity = NotificationSeverity.Info,
                Title = "Test 1",
                Message = "Message 1",
                CreatedAt = DateTime.UtcNow,
                ChannelDeliveries = BuildChannelDeliveries()
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                TypeKey = "test.type",
                Severity = NotificationSeverity.Info,
                Title = "Test 2",
                Message = "Message 2",
                CreatedAt = DateTime.UtcNow,
                ChannelDeliveries = BuildChannelDeliveries()
            }
        };

        var notificationRepository = new Mock<INotificationRepository>();
        notificationRepository
            .Setup(r => r.GetByProfileIdAsync(profileId, true, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedNotifications);

        var preferencesRepository = new Mock<INotificationPreferencesRepository>();
        var typeService = new Mock<INotificationTypeService>();

        var sut = new NotificationService(notificationRepository.Object, preferencesRepository.Object, typeService.Object, NullLogger<NotificationService>.Instance);

        var result = await sut.GetNotificationsAsync(profileId, includeRead: true, includeDeleted: false);

        Assert.Equal(2, result.Count);
        Assert.Same(expectedNotifications, result);
        notificationRepository.Verify(r => r.GetByProfileIdAsync(profileId, true, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetNotificationByIdAsync_ReturnsNotificationFromRepository()
    {
        var notificationId = Guid.NewGuid();
        var expectedNotification = new Notification
        {
            Id = notificationId,
            ProfileId = Guid.NewGuid(),
            TypeKey = "test.type",
            Severity = NotificationSeverity.Info,
            Title = "Test Notification",
            Message = "Test Message",
            CreatedAt = DateTime.UtcNow,
            ChannelDeliveries = BuildChannelDeliveries()
        };

        var notificationRepository = new Mock<INotificationRepository>();
        notificationRepository
            .Setup(r => r.GetByIdAsync(notificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedNotification);

        var preferencesRepository = new Mock<INotificationPreferencesRepository>();
        var typeService = new Mock<INotificationTypeService>();

        var sut = new NotificationService(notificationRepository.Object, preferencesRepository.Object, typeService.Object, NullLogger<NotificationService>.Instance);

        var result = await sut.GetNotificationByIdAsync(notificationId);

        Assert.NotNull(result);
        Assert.Same(expectedNotification, result);
        notificationRepository.Verify(r => r.GetByIdAsync(notificationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetNotificationByIdAsync_ReturnsNullWhenNotFound()
    {
        var notificationId = Guid.NewGuid();

        var notificationRepository = new Mock<INotificationRepository>();
        notificationRepository
            .Setup(r => r.GetByIdAsync(notificationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((INotification?)null);

        var preferencesRepository = new Mock<INotificationPreferencesRepository>();
        var typeService = new Mock<INotificationTypeService>();

        var sut = new NotificationService(notificationRepository.Object, preferencesRepository.Object, typeService.Object, NullLogger<NotificationService>.Instance);

        var result = await sut.GetNotificationByIdAsync(notificationId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCountFromRepository()
    {
        var profileId = Guid.NewGuid();
        const int expectedCount = 5;

        var notificationRepository = new Mock<INotificationRepository>();
        notificationRepository
            .Setup(r => r.GetUnreadCountAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCount);

        var preferencesRepository = new Mock<INotificationPreferencesRepository>();
        var typeService = new Mock<INotificationTypeService>();

        var sut = new NotificationService(notificationRepository.Object, preferencesRepository.Object, typeService.Object, NullLogger<NotificationService>.Instance);

        var result = await sut.GetUnreadCountAsync(profileId);

        Assert.Equal(expectedCount, result);
        notificationRepository.Verify(r => r.GetUnreadCountAsync(profileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_CallsRepositoryMarkAsRead()
    {
        var notificationId = Guid.NewGuid();

        var notificationRepository = new Mock<INotificationRepository>();
        notificationRepository
            .Setup(r => r.MarkAsReadAsync(notificationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var preferencesRepository = new Mock<INotificationPreferencesRepository>();
        var typeService = new Mock<INotificationTypeService>();

        var sut = new NotificationService(notificationRepository.Object, preferencesRepository.Object, typeService.Object, NullLogger<NotificationService>.Instance);

        await sut.MarkAsReadAsync(notificationId);

        notificationRepository.Verify(r => r.MarkAsReadAsync(notificationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_MarksAllUnreadNotifications()
    {
        var profileId = Guid.NewGuid();
        var unreadNotifications = new List<INotification>
        {
            new Notification
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                TypeKey = "test.type",
                Severity = NotificationSeverity.Info,
                Title = "Test 1",
                Message = "Message 1",
                CreatedAt = DateTime.UtcNow,
                ChannelDeliveries = BuildChannelDeliveries()
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                TypeKey = "test.type",
                Severity = NotificationSeverity.Info,
                Title = "Test 2",
                Message = "Message 2",
                CreatedAt = DateTime.UtcNow,
                ChannelDeliveries = BuildChannelDeliveries()
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                TypeKey = "test.type",
                Severity = NotificationSeverity.Info,
                Title = "Test 3",
                Message = "Message 3",
                CreatedAt = DateTime.UtcNow,
                ChannelDeliveries = BuildChannelDeliveries()
            }
        };

        var notificationRepository = new Mock<INotificationRepository>();
        notificationRepository
            .Setup(r => r.GetByProfileIdAsync(profileId, false, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unreadNotifications);
        notificationRepository
            .Setup(r => r.MarkAsReadAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var preferencesRepository = new Mock<INotificationPreferencesRepository>();
        var typeService = new Mock<INotificationTypeService>();

        var sut = new NotificationService(notificationRepository.Object, preferencesRepository.Object, typeService.Object, NullLogger<NotificationService>.Instance);

        await sut.MarkAllAsReadAsync(profileId);

        foreach (var notification in unreadNotifications)
        {
            notificationRepository.Verify(r => r.MarkAsReadAsync(notification.Id, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Fact]
    public async Task MarkAsDeletedAsync_CallsRepositoryMarkAsDeleted()
    {
        var notificationId = Guid.NewGuid();

        var notificationRepository = new Mock<INotificationRepository>();
        notificationRepository
            .Setup(r => r.MarkAsDeletedAsync(notificationId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var preferencesRepository = new Mock<INotificationPreferencesRepository>();
        var typeService = new Mock<INotificationTypeService>();

        var sut = new NotificationService(notificationRepository.Object, preferencesRepository.Object, typeService.Object, NullLogger<NotificationService>.Instance);

        await sut.MarkAsDeletedAsync(notificationId);

        notificationRepository.Verify(r => r.MarkAsDeletedAsync(notificationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteExpiredNotificationsAsync_CallsRepositoryDeleteExpired()
    {
        var notificationRepository = new Mock<INotificationRepository>();
        notificationRepository
            .Setup(r => r.DeleteExpiredAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var preferencesRepository = new Mock<INotificationPreferencesRepository>();
        var typeService = new Mock<INotificationTypeService>();

        var sut = new NotificationService(notificationRepository.Object, preferencesRepository.Object, typeService.Object, NullLogger<NotificationService>.Instance);

        await sut.DeleteExpiredNotificationsAsync();

        notificationRepository.Verify(r => r.DeleteExpiredAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
