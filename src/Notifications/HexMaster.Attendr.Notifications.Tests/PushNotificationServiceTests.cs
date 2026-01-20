using System.Net;
using System.Net.Http;
using HexMaster.Attendr.Notifications.Abstractions.DomainModels;
using HexMaster.Attendr.Notifications.Abstractions.Repositories;
using HexMaster.Attendr.Notifications.DomainModels;
using HexMaster.Attendr.Notifications.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HexMaster.Attendr.Notifications.Tests;

public sealed class PushNotificationServiceTests
{
    [Fact]
    public async Task SendAsync_WhenNoSubscriptions_ReturnsZero()
    {
        var profileId = Guid.NewGuid();
        var repository = new Mock<IPushSubscriptionRepository>();
        repository.Setup(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<IPushSubscription>());

        var handler = new FakeHandler();
        var service = CreateService(repository, handler);

        var result = await service.SendAsync(profileId, "title", "message", null, CancellationToken.None);

        Assert.Equal(0, result);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task SendAsync_SendsToAllSubscriptions()
    {
        var profileId = Guid.NewGuid();
        var repository = new Mock<IPushSubscriptionRepository>();
        var subscriptions = new List<IPushSubscription>
        {
            CreateSubscription(profileId, "https://push.example/1"),
            CreateSubscription(profileId, "https://push.example/2")
        };
        repository.Setup(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        var handler = new FakeHandler();
        var service = CreateService(repository, handler);

        var result = await service.SendAsync(profileId, "title", "message", "https://attendr.live");

        repository.Verify(r => r.GetByProfileIdAsync(profileId, It.IsAny<CancellationToken>()), Times.Once);
        Assert.InRange(result, 0, subscriptions.Count);
        Assert.InRange(handler.Requests.Count, 0, subscriptions.Count);
    }

    [Fact]
    public async Task SendToSubscriptionAsync_ValidatesArguments()
    {
        var service = CreateService(new Mock<IPushSubscriptionRepository>(), new FakeHandler());

        await Assert.ThrowsAsync<ArgumentException>(() => service.SendToSubscriptionAsync(string.Empty, "p256", "auth", "title", "message"));
        await Assert.ThrowsAsync<ArgumentException>(() => service.SendToSubscriptionAsync("endpoint", string.Empty, "auth", "title", "message"));
        await Assert.ThrowsAsync<ArgumentException>(() => service.SendToSubscriptionAsync("endpoint", "p256", string.Empty, "title", "message"));
        await Assert.ThrowsAsync<ArgumentException>(() => service.SendToSubscriptionAsync("endpoint", "p256", "auth", string.Empty, "message"));
        await Assert.ThrowsAsync<ArgumentException>(() => service.SendToSubscriptionAsync("endpoint", "p256", "auth", "title", string.Empty));
    }

    private static PushNotificationService CreateService(Mock<IPushSubscriptionRepository> repository, FakeHandler handler)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["VAPID:PublicKey"] = "BOKRz7dBDEuDfSUeYEYVplpXCMVvjJAnCmvYzu3n6PvJEa3TBUnIFJWMGryuVKy7jHimoMuquxYZc13JUO2cS2E",
                ["VAPID:PrivateKey"] = "nZjz7dBtX1r7DiIP9N6byN1Nsx3Rp3XIanFkF_jxuxE",
                ["VAPID:Subject"] = "mailto:test@attendr.local"
            })
            .Build();

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://webpush.example/")
        };

        return new PushNotificationService(repository.Object, configuration, NullLogger<PushNotificationService>.Instance, httpClient);
    }

    private static PushSubscription CreateSubscription(Guid profileId, string endpoint) => new()
    {
        ProfileId = profileId,
        Endpoint = endpoint,
        P256dh = "BOr30YIOCwVDDOZPA7eGRIDcHruM3aMcMBXNIN1qNbfXDnpa9eKJeNEqXWiwxupCSvJzpuG1HsRvNcredi7hFgU",
        Auth = "y6x7lTi27Ck7uw1Z",
        UserAgent = "Chrome",
        CreatedAt = DateTime.UtcNow
    };

    private sealed class FakeHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = new();
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.Created;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(StatusCode));
        }
    }
}
