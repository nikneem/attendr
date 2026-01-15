using HexMaster.Attendr.Core.Claims;
using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.Abstractions.Repositories;
using HexMaster.Attendr.Notifications.Abstractions.Services;
using HexMaster.Attendr.Notifications.Mappers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HexMaster.Attendr.Notifications.Api.Endpoints;

/// <summary>
/// Endpoints for managing notification preferences.
/// </summary>
public static class NotificationPreferencesEndpoints
{
    public static IEndpointRouteBuilder MapNotificationPreferencesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications/preferences")
            .WithName("NotificationPreferences")
            .WithTags("Notification Preferences")
            .RequireAuthorization();

        group.MapGet("/", GetPreferences)
            .WithName("GetNotificationPreferences")
            .Produces<Abstractions.DTOs.NotificationPreferencesDto>(StatusCodes.Status200OK);

        group.MapPut("/", UpdatePreferences)
            .WithName("UpdateNotificationPreferences")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/do-not-disturb", SetDoNotDisturb)
            .WithName("SetDoNotDisturb")
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    private static async Task<Ok<Abstractions.DTOs.NotificationPreferencesDto>> GetPreferences(
        HttpContext context,
        INotificationPreferencesRepository preferencesRepository,
        INotificationTypeService typeService)
    {
        var profileId = context.GetProfileId();
        var preferences = await preferencesRepository.GetByProfileIdAsync(profileId);

        if (preferences == null)
        {
            // Return defaults
            var allTypes = typeService.GetAllTypes();
            var defaultPrefs = new Abstractions.DomainModels.NotificationPreferences
            {
                ProfileId = profileId,
                TypeChannelPreferences = allTypes.ToDictionary(
                    t => t.TypeKey,
                    t => t.DefaultChannelSettings),
                CreatedAt = DateTime.UtcNow
            };

            return TypedResults.Ok(NotificationDtoMapper.ToDto(defaultPrefs));
        }

        return TypedResults.Ok(NotificationDtoMapper.ToDto(preferences));
    }

    private static async Task<NoContent> UpdatePreferences(
        HttpContext context,
        UpdatePreferencesRequest request,
        INotificationPreferencesRepository preferencesRepository)
    {
        var profileId = context.GetProfileId();
        var preferences = NotificationDtoMapper.ToDomain(profileId, request.TypeChannelPreferences);

        await preferencesRepository.UpsertAsync(preferences);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> SetDoNotDisturb(
        HttpContext context,
        SetDoNotDisturbRequest request,
        INotificationPreferencesRepository preferencesRepository)
    {
        var profileId = context.GetProfileId();
        await preferencesRepository.UpdateDoNotDisturbAsync(profileId, request.DoNotDisturbUntil);
        return TypedResults.NoContent();
    }

    public record UpdatePreferencesRequest(Dictionary<string, Dictionary<string, bool>> TypeChannelPreferences);
    public record SetDoNotDisturbRequest(DateTime? DoNotDisturbUntil);
}
