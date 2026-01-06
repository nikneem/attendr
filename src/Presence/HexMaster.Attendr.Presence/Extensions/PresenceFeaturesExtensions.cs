using Microsoft.Extensions.DependencyInjection;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Presence.Features.CreateConferencePresence;
using HexMaster.Attendr.Presence.Features.GetMyConferences;
using HexMaster.Attendr.Presence.Features.RatePresentation;
using HexMaster.Attendr.Presence.Features.UpdatePresentation;
using HexMaster.Attendr.Presence.Observability;

namespace HexMaster.Attendr.Presence.Extensions;

/// <summary>
/// Extension methods for registering Presence feature services.
/// </summary>
public static class PresenceFeaturesExtensions
{
    /// <summary>
    /// Registers all Presence feature services including command handlers, query handlers, and observability.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
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
}

