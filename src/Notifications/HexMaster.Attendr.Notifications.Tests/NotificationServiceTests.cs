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
}
