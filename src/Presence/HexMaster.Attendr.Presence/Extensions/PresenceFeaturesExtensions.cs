using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using HexMaster.Attendr.Presence.Features.CreateConferencePresence;
using HexMaster.Attendr.Presence.Features.GetMyConferences;
using HexMaster.Attendr.Presence.Features.RatePresentation;
using HexMaster.Attendr.Presence.Features.UpdatePresentation;

namespace HexMaster.Attendr.Presence.Extensions;

public static class PresenceFeaturesExtensions
{
    public static IServiceCollection AddPresenceFeatures(this IServiceCollection services)
    {
        // Register feature services
        services.AddScoped<CreateConferencePresenceService>();
        services.AddScoped<UpdatePresentationService>();
        services.AddScoped<GetRandomPresentationToRateService>();
        services.AddScoped<RatePresentationService>();

        return services;
    }

    public static IEndpointRouteBuilder MapPresenceFeatures(this IEndpointRouteBuilder app)
    {
        // Map feature endpoints
        app.MapGetMyConferencesEndpoint();
        app.MapGetRandomPresentationToRateEndpoint();
        app.MapRatePresentationEndpoint();

        // Map event handler endpoints
        app.MapProfileFollowedConferenceEventHandler();
        app.MapProfilesFollowedConferenceEventHandler();
        app.MapPresentationUpdatedEventHandler();

        return app;
    }
}

