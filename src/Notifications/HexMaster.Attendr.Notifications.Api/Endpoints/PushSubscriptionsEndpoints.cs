using HexMaster.Attendr.Notifications.Abstractions.Repositories;
using HexMaster.Attendr.Notifications.Abstractions.Services;
using HexMaster.Attendr.Notifications.DomainModels;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HexMaster.Attendr.Notifications.Api.Endpoints;

/// <summary>
/// Endpoints for registering push notification subscriptions.
/// </summary>
public static class PushSubscriptionsEndpoints
{
    public static IEndpointRouteBuilder MapPushSubscriptionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications/subscriptions")
            .WithName("PushSubscriptions")
            .WithTags("Push Subscriptions")
            .RequireAuthorization();

        group.MapPost("/", RegisterSubscription)
            .WithName("RegisterPushSubscription")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapGet("/test", SendTestNotification)
            .WithName("SendTestPushNotification")
            .Produces<TestNotificationResponse>(StatusCodes.Status200OK);

        return app;
    }

    private static async Task<Results<NoContent, BadRequest<string>>> RegisterSubscription(
        IProfilesIntegrationService profilesIntegration,
        HttpContext httpContext,
        IPushSubscriptionRepository repository,
        RegisterPushSubscriptionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Endpoint)
            || string.IsNullOrWhiteSpace(request.P256dh)
            || string.IsNullOrWhiteSpace(request.Auth))
        {
            return TypedResults.BadRequest("Endpoint, p256dh, and auth are required");
        }

        var resolvedProfile = await profilesIntegration.GetProfileFromUser(httpContext.User, httpContext.RequestAborted);
        var profileId = Guid.Parse(resolvedProfile.ProfileId);

        var subscription = new PushSubscription
        {
            ProfileId = profileId,
            Endpoint = request.Endpoint,
            P256dh = request.P256dh,
            Auth = request.Auth,
            UserAgent = request.UserAgent ?? string.Empty,
            ExpirationTime = request.ExpirationTimeUtc,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await repository.UpsertAsync(subscription, httpContext.RequestAborted);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<TestNotificationResponse>> SendTestNotification(
        IProfilesIntegrationService profilesIntegration,
        HttpContext httpContext,
        IPushNotificationService pushNotificationService)
    {
        var resolvedProfile = await profilesIntegration.GetProfileFromUser(httpContext.User, httpContext.RequestAborted);
        var profileId = Guid.Parse(resolvedProfile.ProfileId);

        var sentCount = await pushNotificationService.SendAsync(
            profileId,
            "Test Notification",
            "This is a test push notification from Attendr!",
            "https://attendr.com/app/groups",
            httpContext.RequestAborted);

        return TypedResults.Ok(new TestNotificationResponse(sentCount, $"Test notification sent to {sentCount} subscription(s)"));
    }

    public record RegisterPushSubscriptionRequest(
        string Endpoint,
        string P256dh,
        string Auth,
        string? UserAgent,
        DateTime? ExpirationTimeUtc);

    public record TestNotificationResponse(int SentCount, string Message);
}
