using HexMaster.Attendr.Core.Cache.Extensions;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Features.CreateProfile;
using HexMaster.Attendr.Profiles.Features.GetMetrics;
using HexMaster.Attendr.Profiles.Features.GetProfileTopics;
using HexMaster.Attendr.Profiles.Features.SetTopicManualStatus;
using HexMaster.Attendr.Profiles.Features.UpdateProfile;
using HexMaster.Attendr.Profiles.Observability;
using HexMaster.Attendr.Profiles.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.Attendr.Profiles.Extensions;

/// <summary>
/// Extension methods for registering Profiles services in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Attendr Profiles services including command handlers and repositories.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAttendrProfilesServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register metrics
        services.AddSingleton<ProfileMetrics>();

        // Register services
        services.AddSingleton<TopicWeightDecayService>();

        services.AddAttendrCache(configuration);

        // Register command handlers
        services.AddScoped<ICommandHandler<CreateProfileCommand, CreateProfileResult>, CreateProfileCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProfileCommand, UpdateProfileResult>, UpdateProfileCommandHandler>();
        services.AddScoped<ICommandHandler<SetTopicManualStatusCommand, ProfileTopicDto>, SetTopicManualStatusCommandHandler>();

        // Register query handlers
        services.AddScoped<IQueryHandler<GetMetricsQuery, ProfileMetricsDto>, GetMetricsQueryHandler>();
        services.AddScoped<IQueryHandler<GetProfileTopicsQuery, IReadOnlyList<ProfileTopicDto>>, GetProfileTopicsQueryHandler>();

        return services;
    }
}
