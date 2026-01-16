using System.Security.Claims;
using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.Abstractions.Repositories;
using HexMaster.Attendr.Notifications.Abstractions.Services;
using HexMaster.Attendr.Notifications.DomainModels;
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
        ClaimsPrincipal user,
        INotificationPreferencesRepository preferencesRepository,
        INotificationTypeService typeService)
    {
        var profileId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? throw new InvalidOperationException("Profile ID not found in claims"));
        var preferences = await preferencesRepository.GetByProfileIdAsync(profileId);

        if (preferences == null)
        {
            // Return defaults
            var allTypes = typeService.GetAllTypes();
            var defaultPrefs = new DomainModels.NotificationPreferences
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
        ClaimsPrincipal user,
        UpdatePreferencesRequest request,
        INotificationPreferencesRepository preferencesRepository)
    {
        var profileId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? throw new InvalidOperationException("Profile ID not found in claims"));
        var preferences = NotificationDtoMapper.ToDomain(profileId, request.TypeChannelPreferences);

        await preferencesRepository.UpsertAsync(preferences);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> SetDoNotDisturb(
        ClaimsPrincipal user,
        SetDoNotDisturbRequest request,
        INotificationPreferencesRepository preferencesRepository)
    {
        var profileId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? throw new InvalidOperationException("Profile ID not found in claims"));
        await preferencesRepository.UpdateDoNotDisturbAsync(profileId, request.DoNotDisturbUntil);
        return TypedResults.NoContent();
    }

    public record UpdatePreferencesRequest(Dictionary<string, Dictionary<string, bool>> TypeChannelPreferences);
    public record SetDoNotDisturbRequest(DateTime? DoNotDisturbUntil);
}
