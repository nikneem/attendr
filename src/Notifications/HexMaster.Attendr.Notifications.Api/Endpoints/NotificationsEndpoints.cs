using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.Abstractions.Services;
using HexMaster.Attendr.Notifications.Mappers;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HexMaster.Attendr.Notifications.Api.Endpoints;

/// <summary>
/// Endpoints for managing notifications.
/// </summary>
public static class NotificationsEndpoints
{
    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications")
            .WithName("Notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        group.MapGet("/", GetNotifications)
            .WithName("GetNotifications")
            .Produces<IReadOnlyList<Abstractions.DTOs.NotificationDto>>(StatusCodes.Status200OK);

        group.MapGet("/unread/count", GetUnreadCount)
            .WithName("GetUnreadCount")
            .Produces<int>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetNotificationById)
            .WithName("GetNotificationById")
            .Produces<Abstractions.DTOs.NotificationDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/read", MarkAsRead)
            .WithName("MarkNotificationAsRead")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/read-all", MarkAllAsRead)
            .WithName("MarkAllNotificationsAsRead")
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{id:guid}", DeleteNotification)
            .WithName("DeleteNotification")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<Abstractions.DTOs.NotificationDto>>> GetNotifications(
        IProfilesIntegrationService profilesIntegration,
        HttpContext httpContext,
        INotificationService notificationService,
        bool includeRead = true)
    {
        var resolvedProfile = await profilesIntegration.GetProfileFromUser(httpContext.User, httpContext.RequestAborted);
        var profileId = Guid.Parse(resolvedProfile.ProfileId);
        var notifications = await notificationService.GetNotificationsAsync(
            profileId, includeRead, includeDeleted: false);

        var dtos = notifications.Select(NotificationDtoMapper.ToDto).ToList();
        return TypedResults.Ok<IReadOnlyList<Abstractions.DTOs.NotificationDto>>(dtos);
    }

    private static async Task<Ok<int>> GetUnreadCount(
        IProfilesIntegrationService profilesIntegration,
        HttpContext httpContext,
        INotificationService notificationService)
    {
        var resolvedProfile = await profilesIntegration.GetProfileFromUser(httpContext.User, httpContext.RequestAborted);
        var profileId = Guid.Parse(resolvedProfile.ProfileId);
        var count = await notificationService.GetUnreadCountAsync(profileId);
        return TypedResults.Ok(count);
    }

    private static async Task<Results<Ok<Abstractions.DTOs.NotificationDto>, NotFound>> GetNotificationById(
        Guid id,
        INotificationService notificationService)
    {
        var notification = await notificationService.GetNotificationByIdAsync(id);

        if (notification == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(NotificationDtoMapper.ToDto(notification));
    }

    private static async Task<Results<NoContent, NotFound>> MarkAsRead(
        Guid id,
        IProfilesIntegrationService profilesIntegration,
        HttpContext httpContext,
        INotificationService notificationService)
    {
        var resolvedProfile = await profilesIntegration.GetProfileFromUser(httpContext.User, httpContext.RequestAborted);
        var profileId = Guid.Parse(resolvedProfile.ProfileId);
        try
        {
            await notificationService.MarkAsReadAsync(id);
            return TypedResults.NoContent();
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound();
        }
    }

    private static async Task<NoContent> MarkAllAsRead(
        IProfilesIntegrationService profilesIntegration,
        HttpContext httpContext,
        INotificationService notificationService)
    {
        var resolvedProfile = await profilesIntegration.GetProfileFromUser(httpContext.User, httpContext.RequestAborted);
        var profileId = Guid.Parse(resolvedProfile.ProfileId);
        await notificationService.MarkAllAsReadAsync(profileId);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, NotFound>> DeleteNotification(
        Guid id,
        INotificationService notificationService)
    {
        try
        {
            await notificationService.MarkAsDeletedAsync(id);
            return TypedResults.NoContent();
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound();
        }
    }
}
