using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.DomainModels;
using HexMaster.Attendr.Notifications.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace HexMaster.Attendr.Notifications.Tests;

public sealed class EmailNotificationServiceTests
{
    [Fact]
    public async Task SendEmailAsync_ThrowsWhenEmailMissing()
    {
        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance);
        var notification = BuildNotification();

        await Assert.ThrowsAsync<ArgumentException>(() => service.SendEmailAsync(notification, string.Empty));
    }

    [Fact]
    public async Task SendEmailAsync_CompletesWhenValid()
    {
        var service = new EmailNotificationService(NullLogger<EmailNotificationService>.Instance);
        var notification = BuildNotification();

        await service.SendEmailAsync(notification, "user@test.local");
    }

    private static Notification BuildNotification() => new()
    {
        Id = Guid.NewGuid(),
        ProfileId = Guid.NewGuid(),
        TypeKey = "demo.type",
        Severity = NotificationSeverity.Info,
        Title = "title",
        Message = "message",
        CreatedAt = DateTimeOffset.UtcNow,
        ChannelDeliveries = new Dictionary<NotificationChannel, ChannelDeliveryInfo>()
    };
}
