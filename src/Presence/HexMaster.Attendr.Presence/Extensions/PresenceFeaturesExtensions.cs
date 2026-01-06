using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Presence.Features.CreateConferencePresence;
using HexMaster.Attendr.Presence.Features.GetMyConferences;
using HexMaster.Attendr.Presence.Features.RatePresentation;
using HexMaster.Attendr.Presence.Features.UpdatePresentation;
using HexMaster.Attendr.Presence.Observability;

namespace HexMaster.Attendr.Presence.Extensions;

public static class PresenceFeaturesExtensions
{
    public static IServiceCollection AddPresenceFeatures(this IServiceCollection services)
    {
        // Register observability
        services.AddSingleton<PresenceMetrics>();

        // Register command handlers
        services.AddScoped<ICommandHandler<CreateConferencePresenceCommand>, CreateConferencePresenceCommandHandler>();
        services.AddScoped<ICommandHandler<RatePresentationCommand>, RatePresentationCommandHandler>();
        services.AddScoped<ICommandHandler<UpdatePresentationCommand>, UpdatePresentationCommandHandler>();

        // Register query handlers
        services.AddScoped<IQueryHandler<GetMyConferencesQuery, List<MyConferenceResponse>>, GetMyConferencesQueryHandler>();
        services.AddScoped<IQueryHandler<GetRandomPresentationToRateQuery, PresentationToRateDto?>, GetRandomPresentationToRateQueryHandler>();

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

