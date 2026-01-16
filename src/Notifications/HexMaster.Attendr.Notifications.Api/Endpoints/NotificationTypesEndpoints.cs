using HexMaster.Attendr.Notifications.Abstractions.Services;
using HexMaster.Attendr.Notifications.Mappers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HexMaster.Attendr.Notifications.Api.Endpoints;

/// <summary>
/// Endpoints for managing notification types.
/// </summary>
public static class NotificationTypesEndpoints
{
    public static IEndpointRouteBuilder MapNotificationTypesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications/types")
            .WithName("NotificationTypes")
            .WithTags("Notification Types")
            .RequireAuthorization();

        group.MapGet("/", GetAllTypes)
            .WithName("GetAllNotificationTypes")
            .Produces<IReadOnlyList<Abstractions.DTOs.NotificationTypeDto>>(StatusCodes.Status200OK);

        return app;
    }

    private static Ok<IReadOnlyList<Abstractions.DTOs.NotificationTypeDto>> GetAllTypes(
        INotificationTypeService typeService)
    {
        var types = typeService.GetAllTypes();
        var dtos = types.Cast<Models.NotificationType>().Select(NotificationDtoMapper.ToDto).ToList();
        return TypedResults.Ok<IReadOnlyList<Abstractions.DTOs.NotificationTypeDto>>(dtos);
    }
}
