using System.Text.Json;
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
    private readonly HttpClient _httpClient;
    private readonly VapidAuthentication _vapidAuth;

    public PushNotificationService(
        IPushSubscriptionRepository subscriptionRepository,
        IConfiguration configuration,
        ILogger<PushNotificationService> logger,
        HttpClient httpClient)
    {
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        var publicKey = configuration?["VAPID:PublicKey"] ?? throw new InvalidOperationException("VAPID:PublicKey not configured");
        var privateKey = configuration?["VAPID:PrivateKey"] ?? throw new InvalidOperationException("VAPID:PrivateKey not configured");

        _vapidAuth = new VapidAuthentication(publicKey, privateKey);
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
            // Extract the origin from the endpoint URL
            var endpointUri = new Uri(endpoint);
            var audience = $"{endpointUri.Scheme}://{endpointUri.Host}";

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

            // Get VAPID authentication header
            var authHeaderParam = _vapidAuth.GetVapidSchemeAuthenticationHeaderValueParameter(audience);

            // Create HTTP request
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(payloadJson, System.Text.Encoding.UTF8, "application/json"),
                Headers =
                {
                    { "TTL", "24h" },
                    { "Content-Encoding", "aes128gcm" },
                    { "Authorization", $"vapid {authHeaderParam}" }
                }
            };

            // Send the notification
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Gone)
            {
                // 410 Gone - subscription is no longer valid
                _logger.LogInformation("Subscription {Endpoint} is no longer valid (410 Gone), removing it", endpoint);
                await _subscriptionRepository.DeleteAsync(Guid.Empty, endpoint, cancellationToken);
            }
            else if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Failed to send push notification to {Endpoint}: {StatusCode} {ReasonPhrase}",
                    endpoint,
                    response.StatusCode,
                    response.ReasonPhrase);
                throw new InvalidOperationException($"Push notification failed with status {response.StatusCode}");
            }

            _logger.LogInformation("Successfully sent push notification to {Endpoint}", endpoint);
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
