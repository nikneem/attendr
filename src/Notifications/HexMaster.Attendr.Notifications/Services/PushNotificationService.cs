using System.Text.Json;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using HexMaster.Attendr.Notifications.Abstractions.Repositories;
using HexMaster.Attendr.Notifications.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Notifications.Services;

/// <summary>
/// Implementation of IPushNotificationService using WebPush library.
/// </summary>
public sealed class PushNotificationService : IPushNotificationService
{
    private readonly IPushSubscriptionRepository _subscriptionRepository;
    private readonly ILogger<PushNotificationService> _logger;
    private readonly PushServiceClient _pushClient;

    public PushNotificationService(
        IPushSubscriptionRepository subscriptionRepository,
        IConfiguration configuration,
        ILogger<PushNotificationService> logger,
        HttpClient httpClient)
    {
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var publicKey = configuration?["VAPID:PublicKey"] ?? throw new InvalidOperationException("VAPID:PublicKey not configured");
        var privateKey = configuration?["VAPID:PrivateKey"] ?? throw new InvalidOperationException("VAPID:PrivateKey not configured");
        var subject = configuration?["VAPID:Subject"] ?? "mailto:support@attendr.io";

        _pushClient = new PushServiceClient(httpClient)
        {
            DefaultAuthentication = new VapidAuthentication(publicKey, privateKey)
            {
                Subject = subject
            }
        };
    }

    public async Task<int> SendAsync(
        Guid profileId,
        string title,
        string message,
        string? url = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var subscriptions = await _subscriptionRepository.GetByProfileIdAsync(profileId, cancellationToken);

        if (subscriptions.Count == 0)
        {
            _logger.LogInformation("No push subscriptions found for profile {ProfileId}", profileId);
            return 0;
        }

        var successCount = 0;
        foreach (var subscription in subscriptions)
        {
            try
            {
                await SendToSubscriptionAsync(
                    subscription.Endpoint,
                    subscription.P256dh,
                    subscription.Auth,
                    title,
                    message,
                    url,
                    cancellationToken);

                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to send push notification to subscription {Endpoint} for profile {ProfileId}",
                    subscription.Endpoint,
                    profileId);
            }
        }

        _logger.LogInformation(
            "Sent push notification to {SuccessCount} of {TotalCount} subscriptions for profile {ProfileId}",
            successCount,
            subscriptions.Count,
            profileId);

        return successCount;
    }

    public async Task SendToSubscriptionAsync(
        string endpoint,
        string p256dh,
        string auth,
        string title,
        string message,
        string? url = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(p256dh);
        ArgumentException.ThrowIfNullOrWhiteSpace(auth);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        try
        {
            // Build the payload
            var payload = new
            {
                title,
                body = message,
                icon = "/logo/icon-192x192.png",
                badge = "/logo/icon-192x192.png",
                url
            };

            var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Create PushSubscription object
            var pushSubscription = new PushSubscription
            {
                Endpoint = endpoint,
                Keys = new Dictionary<string, string>
                {
                    { "p256dh", p256dh },
                    { "auth", auth }
                }
            };

            // Create PushMessage with the payload
            var pushMessage = new PushMessage(payloadJson);

            // Send the notification using PushServiceClient
            await _pushClient.RequestPushMessageDeliveryAsync(pushSubscription, pushMessage, cancellationToken);

            _logger.LogInformation("Successfully sent push notification to {Endpoint}", endpoint);
        }
        catch (PushServiceClientException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone)
        {
            // 410 Gone - subscription is no longer valid
            _logger.LogInformation("Subscription {Endpoint} is no longer valid (410 Gone), removing it", endpoint);
            await _subscriptionRepository.DeleteAsync(Guid.Empty, endpoint, cancellationToken);
        }
        catch (PushServiceClientException ex)
        {
            _logger.LogError(
                ex,
                "Failed to send push notification to {Endpoint}: {StatusCode} {ReasonPhrase}",
                endpoint,
                ex.StatusCode,
                ex.Message);
            throw new InvalidOperationException($"Push notification failed with status {ex.StatusCode}", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP exception when sending push notification to {Endpoint}", endpoint);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception when sending push notification to {Endpoint}", endpoint);
            throw;
        }
    }
}
