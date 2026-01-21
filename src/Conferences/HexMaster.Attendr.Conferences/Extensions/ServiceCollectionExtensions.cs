using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Abstractions.Services;
using HexMaster.Attendr.Conferences.CreateConference;
using HexMaster.Attendr.Conferences.DeleteConference;
using HexMaster.Attendr.Conferences.FollowConference;
using HexMaster.Attendr.Conferences.GetConference;
using HexMaster.Attendr.Conferences.ListConferences;
using HexMaster.Attendr.Conferences.Observability;
using HexMaster.Attendr.Conferences.Services;
using HexMaster.Attendr.Conferences.UpdateConference;
using HexMaster.Attendr.Core.CommandHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace HexMaster.Attendr.Conferences.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAttendrConferencesServices(this IServiceCollection services)
    {
        // Register metrics
        services.AddSingleton<ConferenceMetrics>();

        // Register services
        services.AddScoped<ISessionizeSyncService, SessionizeSyncService>();
        services.AddScoped<PresentationTopicsAnalysisService>();

        // Register background services
        services.AddHostedService<PresentationAnalysisBackgroundService>();

        // Register command handlers
        services.AddScoped<ICommandHandler<CreateConferenceCommand, CreateConferenceResult>, CreateConferenceCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateConferenceCommand, ConferenceDetailsDto>, UpdateConferenceCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteConferenceCommand, bool>, DeleteConferenceCommandHandler>();
        services.AddScoped<ICommandHandler<FollowConferenceCommand>, FollowConferenceCommandHandler>();

        // Register query handlers
        services.AddScoped<IQueryHandler<ListConferencesQuery, ListConferencesResult>, ListConferencesQueryHandler>();
        services.AddScoped<IQueryHandler<GetConferenceQuery, ConferenceDetailsDto?>, GetConferenceQueryHandler>();

        return services;
    }

    /// <summary>
    /// Adds Semantic Kernel services for AI-powered topic analysis.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSemanticKernelForConferences(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"];
        var deploymentName = configuration["AzureOpenAI:DeploymentName"];
        var apiKey = configuration["AzureOpenAI:ApiKey"];

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(deploymentName) || string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Azure OpenAI configuration is missing. Ensure AzureOpenAI:Endpoint, AzureOpenAI:DeploymentName, and AzureOpenAI:ApiKey are configured.");
        }

        services.AddKernel()
            .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

        return services;
    }
}
