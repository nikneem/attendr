using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Abstractions.Services;
using HexMaster.Attendr.Conferences.Features.CreateConference;
using HexMaster.Attendr.Conferences.Features.Speakers.ListSpeakers;
using HexMaster.Attendr.Conferences.Features.Speakers.GetSpeaker;
using HexMaster.Attendr.Conferences.Features.Speakers.CreateSpeaker;
using HexMaster.Attendr.Conferences.Features.Speakers.UpdateSpeaker;
using HexMaster.Attendr.Conferences.Features.Speakers.DeleteSpeaker;
using HexMaster.Attendr.Conferences.Features.Rooms.ListRooms;
using HexMaster.Attendr.Conferences.Features.Rooms.GetRoom;
using HexMaster.Attendr.Conferences.Features.Rooms.CreateRoom;
using HexMaster.Attendr.Conferences.Features.Rooms.UpdateRoom;
using HexMaster.Attendr.Conferences.Features.Rooms.DeleteRoom;
using HexMaster.Attendr.Conferences.Features.Presentations.ListPresentations;
using HexMaster.Attendr.Conferences.Features.Presentations.GetPresentation;
using HexMaster.Attendr.Conferences.Features.Presentations.CreatePresentation;
using HexMaster.Attendr.Conferences.Features.Presentations.UpdatePresentation;
using HexMaster.Attendr.Conferences.Features.Presentations.DeletePresentation;
using HexMaster.Attendr.Conferences.Features.DeleteConference;
using HexMaster.Attendr.Conferences.Features.FollowConference;
using HexMaster.Attendr.Conferences.Features.GetConference;
using HexMaster.Attendr.Conferences.Features.ListConferences;
using HexMaster.Attendr.Conferences.Features.CreateTopic;
using HexMaster.Attendr.Conferences.Features.GetTopic;
using HexMaster.Attendr.Conferences.Features.ListTopics;
using HexMaster.Attendr.Conferences.Features.UpdateTopic;
using HexMaster.Attendr.Conferences.Features.DeleteTopic;
using HexMaster.Attendr.Conferences.Features.GetMetrics;
using HexMaster.Attendr.Conferences.Observability;
using HexMaster.Attendr.Conferences.Plugins;
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

        // Register metrics query handler
        services.AddScoped<IQueryHandler<GetMetricsQuery, ConferenceMetricsDto>, GetMetricsQueryHandler>();

        // Register topic query handlers
        services.AddScoped<IQueryHandler<GetTopicQuery, TopicDto?>, GetTopicQueryHandler>();
        services.AddScoped<IQueryHandler<ListTopicsQuery, ListTopicsResult>, ListTopicsQueryHandler>();

        // Register speaker handlers
        services.AddScoped<IQueryHandler<ListSpeakersQuery, IReadOnlyList<ConferenceSpeakerDto>>, ListSpeakersQueryHandler>();
        services.AddScoped<IQueryHandler<GetSpeakerQuery, ConferenceSpeakerDto?>, GetSpeakerQueryHandler>();
        services.AddScoped<ICommandHandler<CreateSpeakerCommand, ConferenceSpeakerDto>, CreateSpeakerCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateSpeakerCommand, ConferenceSpeakerDto>, UpdateSpeakerCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteSpeakerCommand, bool>, DeleteSpeakerCommandHandler>();

        // Register room handlers
        services.AddScoped<IQueryHandler<ListRoomsQuery, IReadOnlyList<ConferenceRoomDto>>, ListRoomsQueryHandler>();
        services.AddScoped<IQueryHandler<GetRoomQuery, ConferenceRoomDto?>, GetRoomQueryHandler>();
        services.AddScoped<ICommandHandler<CreateRoomCommand, ConferenceRoomDto>, CreateRoomCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateRoomCommand, ConferenceRoomDto>, UpdateRoomCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteRoomCommand, bool>, DeleteRoomCommandHandler>();

        // Register presentation handlers
        services.AddScoped<IQueryHandler<ListPresentationsQuery, IReadOnlyList<ConferencePresentationDto>>, ListPresentationsQueryHandler>();
        services.AddScoped<IQueryHandler<GetPresentationQuery, ConferencePresentationDto?>, GetPresentationQueryHandler>();
        services.AddScoped<ICommandHandler<CreatePresentationCommand, ConferencePresentationDto>, CreatePresentationCommandHandler>();
        services.AddScoped<ICommandHandler<UpdatePresentationCommand, ConferencePresentationDto>, UpdatePresentationCommandHandler>();
        services.AddScoped<ICommandHandler<DeletePresentationCommand, bool>, DeletePresentationCommandHandler>();

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

        // Register the TopicsPlugin as a singleton
        services.AddSingleton<TopicsPlugin>();

        // Add Kernel with Azure OpenAI and register the TopicsPlugin
        services.AddKernel()
            .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey)
            .Plugins.AddFromType<TopicsPlugin>();

        return services;
    }
}
