using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Abstractions.Services;
using HexMaster.Attendr.Conferences.Features.CreateConference;
using HexMaster.Attendr.Conferences.Features.DeleteConference;
using HexMaster.Attendr.Conferences.Features.FollowConference;
using HexMaster.Attendr.Conferences.Features.GetConference;
using HexMaster.Attendr.Conferences.Features.ListConferences;
using HexMaster.Attendr.Conferences.Features.CreateTopic;
using HexMaster.Attendr.Conferences.Features.GetTopic;
using HexMaster.Attendr.Conferences.Features.ListTopics;
using HexMaster.Attendr.Conferences.Features.UpdateTopic;
using HexMaster.Attendr.Conferences.Features.DeleteTopic;
using HexMaster.Attendr.Conferences.Observability;
using HexMaster.Attendr.Conferences.Services;
using HexMaster.Attendr.Conferences.Features.UpdateConference;
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

        // Register topic command handlers
        services.AddScoped<ICommandHandler<CreateTopicCommand, TopicDto>, CreateTopicCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateTopicCommand, TopicDto>, UpdateTopicCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteTopicCommand, bool>, DeleteTopicCommandHandler>();

        // Register query handlers
        services.AddScoped<IQueryHandler<ListConferencesQuery, ListConferencesResult>, ListConferencesQueryHandler>();
        services.AddScoped<IQueryHandler<GetConferenceQuery, ConferenceDetailsDto?>, GetConferenceQueryHandler>();

        // Register topic query handlers
        services.AddScoped<IQueryHandler<GetTopicQuery, TopicDto?>, GetTopicQueryHandler>();
        services.AddScoped<IQueryHandler<ListTopicsQuery, ListTopicsResult>, ListTopicsQueryHandler>();

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
