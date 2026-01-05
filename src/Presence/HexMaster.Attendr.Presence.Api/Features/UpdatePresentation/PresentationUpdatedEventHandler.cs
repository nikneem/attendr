using HexMaster.Attendr.Core.Constants;
using HexMaster.Attendr.IntegrationEvents.Events;

namespace HexMaster.Attendr.Presence.Api.Features.UpdatePresentation;

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
        UpdatePresentationService service,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            await service.ExecuteAsync(@event, cancellationToken);

            return Results.Ok(new { message = "Presentation updated", conferenceId = @event.ConferenceId, presentationId = @event.PresentationId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling PresentationUpdated for conference {ConferenceId}, presentation {PresentationId}", @event.ConferenceId, @event.PresentationId);
            return Results.BadRequest(new { error = "Failed to handle event", details = ex.Message });
        }
    }
}
