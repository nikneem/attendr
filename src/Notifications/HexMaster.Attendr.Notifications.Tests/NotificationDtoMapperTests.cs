using HexMaster.Attendr.Notifications.Abstractions.DTOs;
using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.Abstractions.DomainModels;
using HexMaster.Attendr.Notifications.DomainModels;
using HexMaster.Attendr.Notifications.Mappers;
using HexMaster.Attendr.Notifications.Models;
using Moq;

namespace HexMaster.Attendr.Notifications.Tests;

public sealed class NotificationDtoMapperTests
{
    [Fact]
    public void ToDto_MapsNotificationFields()
    {
        var now = DateTimeOffset.UtcNow;
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            ProfileId = Guid.NewGuid(),
            TypeKey = "demo.type",
            Severity = NotificationSeverity.Warning,
            Title = "Title",
            Message = "Message",
            Url = "https://attendr.live",
            ActorId = Guid.NewGuid(),
            EntityRefs = new Dictionary<string, string> { ["key"] = "value" },
            Count = 2,
            CreatedAt = now,
            LastOccurredAt = now.AddMinutes(1),
            ReadAt = now.AddMinutes(2),
            ChannelDeliveries = new Dictionary<NotificationChannel, ChannelDeliveryInfo>
            {
                [NotificationChannel.InApp] = new ChannelDeliveryInfo { Enabled = true, Status = DeliveryStatus.Delivered, DeliveredAt = now.AddSeconds(30) },
                [NotificationChannel.Push] = new ChannelDeliveryInfo { Enabled = false, Status = DeliveryStatus.Skipped }
            }
        };

        var dto = NotificationDtoMapper.ToDto(notification);

        Assert.Equal(notification.Id, dto.Id);
        Assert.Equal(notification.ProfileId, dto.ProfileId);
        Assert.Equal(notification.TypeKey, dto.TypeKey);
        Assert.Equal("Warning", dto.Severity);
        Assert.Equal(notification.Title, dto.Title);
        Assert.Equal(notification.Message, dto.Message);
        Assert.Equal(notification.Url, dto.Url);
        Assert.Equal(notification.ActorId, dto.ActorId);
        Assert.Equal(notification.Count, dto.Count);
        Assert.NotNull(dto.ChannelDeliveries);
        var channelDeliveries = dto.ChannelDeliveries!;
        Assert.Equal(notification.ChannelDeliveries.Count, channelDeliveries.Count);
        Assert.True(channelDeliveries.TryGetValue(nameof(NotificationChannel.InApp), out var inApp));
        Assert.NotNull(inApp);
        Assert.Equal("Delivered", inApp!.Status);
        Assert.True(channelDeliveries.TryGetValue(nameof(NotificationChannel.Push), out var push));
        Assert.NotNull(push);
        Assert.Equal("Skipped", push!.Status);
    }

    [Fact]
    public void ToDto_InterfaceImplementationNotNotification_Throws()
    {
        var fake = new Mock<INotification>().Object;
        Assert.Throws<InvalidOperationException>(() => NotificationDtoMapper.ToDto(fake));
    }

    [Fact]
    public void ToDto_MapsPreferences()
    {
        var profileId = Guid.NewGuid();
        var prefs = new NotificationPreferences
        {
            ProfileId = profileId,
            TypeChannelPreferences = new Dictionary<string, Dictionary<NotificationChannel, bool>>
            {
                ["demo.type"] = new()
                {
                    [NotificationChannel.InApp] = true,
                    [NotificationChannel.Email] = false,
                    [NotificationChannel.Push] = true
                }
            },
            DoNotDisturbUntil = DateTimeOffset.UtcNow.AddHours(1),
            CreatedAt = DateTimeOffset.UtcNow
        };

        var dto = NotificationDtoMapper.ToDto(prefs);

        Assert.Equal(profileId, dto.ProfileId);
        Assert.NotNull(dto.TypeChannelPreferences);
        var typeChannelPreferences = dto.TypeChannelPreferences!;
        Assert.True(typeChannelPreferences.TryGetValue("demo.type", out var channelPrefs));
        Assert.NotNull(channelPrefs);
        Assert.True(channelPrefs![nameof(NotificationChannel.InApp)]);
        Assert.False(channelPrefs[nameof(NotificationChannel.Email)]);
        Assert.Equal(prefs.DoNotDisturbUntil, dto.DoNotDisturbUntil);
    }

    [Fact]
    public void ToDomain_ParsesChannelNames()
    {
        var profileId = Guid.NewGuid();
        var prefs = NotificationDtoMapper.ToDomain(profileId, new Dictionary<string, Dictionary<string, bool>>
        {
            ["demo.type"] = new()
            {
                [nameof(NotificationChannel.Push)] = true,
                [nameof(NotificationChannel.Email)] = false
            }
        });

        Assert.Equal(profileId, prefs.ProfileId);
        Assert.True(prefs.TypeChannelPreferences["demo.type"][NotificationChannel.Push]);
        Assert.False(prefs.TypeChannelPreferences["demo.type"][NotificationChannel.Email]);
    }
}
