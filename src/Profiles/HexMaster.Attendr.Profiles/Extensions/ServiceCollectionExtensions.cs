using HexMaster.Attendr.Core.Cache.Extensions;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.CreateProfile;
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

        services.AddAttendrCache(configuration);

        // Register command handlers
        services.AddScoped<ICommandHandler<CreateProfileCommand, CreateProfileResult>, CreateProfileCommandHandler>();

        return services;
    }
}
