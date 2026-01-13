using Microsoft.Extensions.DependencyInjection;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Presence.Features.CheckIn;
using HexMaster.Attendr.Presence.Features.CreateConferencePresence;
using HexMaster.Attendr.Presence.Features.GetConferenceAttendance;
using HexMaster.Attendr.Presence.Features.GetConferenceWithPresentations;
using HexMaster.Attendr.Presence.Features.GetMyConferences;
using HexMaster.Attendr.Presence.Features.GetCurrentConferences;
using HexMaster.Attendr.Presence.Features.RatePresentation;
using HexMaster.Attendr.Presence.Features.SetPreferredPresentation;
using HexMaster.Attendr.Presence.Features.UnfollowConference;
using HexMaster.Attendr.Presence.Features.UpdateAttendance;
using HexMaster.Attendr.Presence.Features.UpdateConference;
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
        services.AddScoped<ICommandHandler<UpdateConferenceCommand>, UpdateConferenceCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateAttendanceCommand>, UpdateAttendanceCommandHandler>();
        services.AddScoped<ICommandHandler<UnfollowConferenceCommand>, UnfollowConferenceCommandHandler>();
        services.AddScoped<ICommandHandler<CheckInCommand>, CheckInCommandHandler>();
        services.AddScoped<ICommandHandler<SetPreferredPresentationCommand>, SetPreferredPresentationCommandHandler>();

        // Register query handlers
        services.AddScoped<IQueryHandler<GetMyConferencesQuery, List<MyConferenceResponse>>, GetMyConferencesQueryHandler>();
        services.AddScoped<IQueryHandler<GetCurrentConferencesQuery, List<CurrentConferenceResponse>>, GetCurrentConferencesQueryHandler>();
        services.AddScoped<IQueryHandler<GetRandomPresentationToRateQuery, PresentationToRateDto?>, GetRandomPresentationToRateQueryHandler>();
        services.AddScoped<IQueryHandler<GetConferenceAttendanceQuery, ConferenceAttendanceDto>, GetConferenceAttendanceQueryHandler>();
        services.AddScoped<IQueryHandler<GetConferenceWithPresentationsQuery, ConferenceWithPresentationsResponse>, GetConferenceWithPresentationsQueryHandler>();

        return services;
    }
}

