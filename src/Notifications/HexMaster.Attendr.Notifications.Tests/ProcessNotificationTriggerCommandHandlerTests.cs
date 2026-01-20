using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.Abstractions.Services;
using HexMaster.Attendr.Notifications.DomainModels;
using HexMaster.Attendr.Notifications.Features.ProcessNotificationTrigger;
using HexMaster.Attendr.Notifications.Models;
using HexMaster.Attendr.Notifications.Services;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Integrations.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Notifications.Tests;

public sealed class ProcessNotificationTriggerCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTypeIsUnknown_StopsProcessing()
    {
        var notificationService = new Mock<INotificationService>();
        var typeService = new Mock<INotificationTypeService>();
        typeService.Setup(s => s.GetTypeByKey(It.IsAny<string>()))
            .Returns((Abstractions.Models.INotificationType?)null);

        var handler = BuildHandler(notificationService, typeService);
        var command = new ProcessNotificationTriggerCommand(Guid.NewGuid(), "missing", "title", "message");

        await handler.Handle(command, CancellationToken.None);

        notificationService.Verify(s => s.CreateNotificationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDndIsActive_SkipsAllChannels()
    {
        var notificationService = new Mock<INotificationService>();
        var typeService = new Mock<INotificationTypeService>();
        typeService.Setup(s => s.GetTypeByKey(It.IsAny<string>()))
            .Returns(CreateBroadcastType());

        var preferencesCache = new Mock<INotificationPreferencesCacheService>();
        preferencesCache.Setup(c => c.GetOrFetchPreferencesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationPreferences
            {
                ProfileId = Guid.NewGuid(),
                TypeChannelPreferences = new Dictionary<string, Dictionary<NotificationChannel, bool>>(),
                DoNotDisturbUntil = DateTime.UtcNow.AddMinutes(10),
                CreatedAt = DateTime.UtcNow
            });

        var handler = BuildHandler(notificationService, typeService, preferencesCache);
        var command = new ProcessNotificationTriggerCommand(Guid.NewGuid(), "any", "title", "message");

        await handler.Handle(command, CancellationToken.None);

        notificationService.Verify(s => s.CreateNotificationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SendsEnabledChannels()
    {
        var profileId = Guid.NewGuid();
        var typeKey = "demo.type";
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            TypeKey = typeKey,
            Severity = NotificationSeverity.Info,
            Title = "title",
            Message = "message",
            CreatedAt = DateTime.UtcNow,
            ChannelDeliveries = new Dictionary<NotificationChannel, ChannelDeliveryInfo>()
        };

        var notificationService = new Mock<INotificationService>();
        notificationService
            .Setup(s => s.CreateNotificationAsync(profileId, typeKey, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        var typeService = new Mock<INotificationTypeService>();
        typeService.Setup(s => s.GetTypeByKey(typeKey))
            .Returns(CreateBroadcastType(defaultEnabled: true));

        var preferencesCache = new Mock<INotificationPreferencesCacheService>();
        preferencesCache.Setup(c => c.GetOrFetchPreferencesAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationPreferences
            {
                ProfileId = profileId,
                TypeChannelPreferences = new Dictionary<string, Dictionary<NotificationChannel, bool>>
                {
                    [typeKey] = new()
                    {
                        [NotificationChannel.Email] = true,
                        [NotificationChannel.Push] = true
                    }
                },
                CreatedAt = DateTime.UtcNow
            });

        var profileService = new Mock<IProfilesIntegrationService>();
        profileService.Setup(s => s.GetProfileDetails(profileId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProfileDetailsDto(profileId.ToString(), "Display", "First", "Last", "user@test.local", null));

        var emailService = new Mock<IEmailNotificationService>();
        var pushService = new Mock<IPushNotificationService>();
        pushService.Setup(p => p.SendAsync(profileId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = BuildHandler(notificationService, typeService, preferencesCache, profileService, emailService, pushService);
        var command = new ProcessNotificationTriggerCommand(profileId, typeKey, "title", "message", "https://attendr.live");

        await handler.Handle(command, CancellationToken.None);

        notificationService.Verify(s => s.CreateNotificationAsync(profileId, typeKey, "title", "message", "https://attendr.live", null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        emailService.Verify(s => s.SendEmailAsync(notification, "user@test.local", It.IsAny<CancellationToken>()), Times.Once);
        pushService.Verify(s => s.SendAsync(profileId, "title", "message", "https://attendr.live", It.IsAny<CancellationToken>()), Times.Once);
    }

    private static NotificationType CreateBroadcastType(bool defaultEnabled = false) => new()
    {
        TypeKey = "demo.type",
        DisplayName = "Demo",
        Description = "Demo type",
        Severity = NotificationSeverity.Info,
        AllowsStacking = false,
        DefaultChannelSettings = new Dictionary<NotificationChannel, bool>
        {
            [NotificationChannel.Email] = defaultEnabled,
            [NotificationChannel.Push] = defaultEnabled,
            [NotificationChannel.InApp] = true
        },
        AvailableChannels = new Dictionary<NotificationChannel, bool>
        {
            [NotificationChannel.Email] = true,
            [NotificationChannel.Push] = true,
            [NotificationChannel.InApp] = true
        }
    };

    private static ProcessNotificationTriggerCommandHandler BuildHandler(
        Mock<INotificationService>? notificationService = null,
        Mock<INotificationTypeService>? typeService = null,
        Mock<INotificationPreferencesCacheService>? preferencesCache = null,
        Mock<IProfilesIntegrationService>? profilesIntegrationService = null,
        Mock<IEmailNotificationService>? emailService = null,
        Mock<IPushNotificationService>? pushService = null)
    {
        return new ProcessNotificationTriggerCommandHandler(
            (notificationService ?? new Mock<INotificationService>()).Object,
            (typeService ?? new Mock<INotificationTypeService>()).Object,
            (preferencesCache ?? new Mock<INotificationPreferencesCacheService>()).Object,
            (profilesIntegrationService ?? new Mock<IProfilesIntegrationService>()).Object,
            (emailService ?? new Mock<IEmailNotificationService>()).Object,
            (pushService ?? new Mock<IPushNotificationService>()).Object,
            NullLogger<ProcessNotificationTriggerCommandHandler>.Instance);
    }
}
