using Dapr;
using HexMaster.Attendr.Aspire.AppHost;
using HexMaster.Attendr.IntegrationEvents.Constants;
using HexMaster.Attendr.IntegrationEvents.Events.Conferences;
using HexMaster.Attendr.IntegrationEvents.Events.Profiles;
using Microsoft.AspNetCore.Mvc;

namespace HexMaster.Attendr.Profiles.Api.Endpoints;

/// <summary>
/// Integration event handler endpoints for Dapr pub/sub.
/// </summary>
public static class EventHandlerEndpoints
{
    /// <summary>
    /// Maps event handler endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapEventHandlerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profiles-integration/events")
            .WithName("ProfilesEventHandlers")
            .ExcludeFromDescription();

        group.MapPost("/topic-interest", EventHandlers.HandleProfileTopicInterestEvent)
            .WithName("HandleProfileTopicInterestEvent")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.ProfileTopicInterest)
            .Accepts<ProfileTopicInterestEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group.MapPost("/conference-presentations-imported", EventHandlers.HandleConferencePresentationsImportedEvent)
            .WithName("HandleConferencePresentationsImportedEvent")
            .WithTopic(AspireConstants.Dapr.PubSubName, IntegrationEventTopics.ConferencePresentationsImported)
            .Accepts<ConferencePresentationsImportedEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        return app;
    }
}
