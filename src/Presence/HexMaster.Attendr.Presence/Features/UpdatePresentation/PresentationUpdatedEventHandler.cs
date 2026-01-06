using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using HexMaster.Attendr.Core.Constants;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.IntegrationEvents.Events;

namespace HexMaster.Attendr.Presence.Features.UpdatePresentation;

public static class PresentationUpdatedEventHandler
{
    public static IEndpointRouteBuilder MapPresentationUpdatedEventHandler(this IEndpointRouteBuilder app)
    {
        app.MapPost("/events/presentation-updated", HandleAsync)
            .WithName("HandlePresentationUpdated")
            .WithTopic(DaprConstants.PubSub.DaprPubSubName, DaprConstants.Topics.PresentationUpdated)
            .Accepts<PresentationUpdatedEvent>("application/cloudevents+json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> HandleAsync(
        PresentationUpdatedEvent @event,
        ICommandHandler<UpdatePresentationCommand> handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("PresentationUpdatedEventHandler");
        try
        {
            await handler.Handle(new UpdatePresentationCommand(@event), cancellationToken);

            return Results.Ok(new { message = "Presentation updated", conferenceId = @event.ConferenceId, presentationId = @event.PresentationId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling PresentationUpdated for conference {ConferenceId}, presentation {PresentationId}", @event.ConferenceId, @event.PresentationId);
            return Results.BadRequest(new { error = "Failed to handle event", details = ex.Message });
        }
    }
}


