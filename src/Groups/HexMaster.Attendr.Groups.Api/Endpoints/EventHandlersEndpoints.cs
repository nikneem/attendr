using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Constants;
using HexMaster.Attendr.Groups.ProcessProfileCheckedIn;
using HexMaster.Attendr.IntegrationEvents.Constants;
using HexMaster.Attendr.IntegrationEvents.Events;

namespace HexMaster.Attendr.Groups.Api.Endpoints;

/// <summary>
/// Extension methods to map Dapr event handler endpoints.
/// </summary>
public static class EventHandlersEndpoints
{
    /// <summary>
    /// Maps the event handler endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static IEndpointRouteBuilder MapEventHandlersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/EventHandlers")
            .WithName("EventHandlers");

        group.MapPost("/ProfileCheckedInHandler", HandleProfileCheckedIn)
            .WithName("HandleProfileCheckedIn")
            .WithTopic(DaprConstants.PubSub.Name, IntegrationEventTopics.ProfileCheckedIn)
            .Accepts<ProfileCheckedInEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> HandleProfileCheckedIn(
        ProfileCheckedInEvent @event,
        ICommandHandler<ProcessProfileCheckedInCommand> handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("ProfileCheckedInEventHandler");
        try
        {
            logger.LogInformation(
                "Processing ProfileCheckedIn event for profile {ProfileId}, presentation {PresentationId}, isCheckedIn: {IsCheckedIn}",
                @event.ProfileId,
                @event.PresentationId,
                @event.IsCheckedIn);

            await handler.Handle(new ProcessProfileCheckedInCommand(@event), cancellationToken);

            return Results.Ok(new
            {
                message = "Profile check-in processed and group activities updated",
                profileId = @event.ProfileId,
                presentationId = @event.PresentationId,
                isCheckedIn = @event.IsCheckedIn
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ProfileCheckedIn for profile {ProfileId}, presentation {PresentationId}",
                @event.ProfileId,
                @event.PresentationId);
            return Results.BadRequest(new { error = "Failed to handle event", details = ex.Message });
        }
    }
}
