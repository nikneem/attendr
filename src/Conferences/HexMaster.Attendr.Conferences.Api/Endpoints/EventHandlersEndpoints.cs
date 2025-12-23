using Dapr;
using HexMaster.Attendr.Conferences.Abstractions.Services;
using HexMaster.Attendr.IntegrationEvents.Events;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

public static class EventHandlersEndpoints
{
    public static IEndpointRouteBuilder MapEventHandlersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/EventHandlers")
            .WithName("EventHandlers");

        group.MapPost("/ConferenceCreatedHandler", ConferenceCreatedHandler)
            .WithName("ConferenceCreatedHandler")
            .AllowAnonymous()
            .WithTopic("pubsub", "conference.created")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/ConferenceUpdatedHandler", ConferenceUpdatedHandler)
            .WithName("ConferenceUpdatedHandler")
            .AllowAnonymous()
            .WithTopic("pubsub", "conference.updated")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> ConferenceCreatedHandler(
        ConferenceCreatedEvent @event,
        ISessionizeSyncService sessionizeSyncService,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Processing ConferenceCreated event for conference {ConferenceId}", @event.ConferenceId);

            var result = await sessionizeSyncService.SynchronizeConferenceAsync(@event.ConferenceId, cancellationToken);

            if (result == null)
            {
                logger.LogWarning("Conference {ConferenceId} not found", @event.ConferenceId);
                return Results.BadRequest(new { error = "Conference not found" });
            }

            return Results.Ok(new
            {
                message = "Conference synchronized successfully",
                conferenceId = result.ConferenceId,
                speakersCount = result.SpeakersCount,
                roomsCount = result.RoomsCount,
                presentationsCount = result.PresentationsCount
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing ConferenceCreated event for conference {ConferenceId}", @event.ConferenceId);
            return Results.BadRequest(new { error = "Failed to process conference created event", details = ex.Message });
        }
    }

    private static async Task<IResult> ConferenceUpdatedHandler(
        ConferenceUpdatedEvent @event,
        ISessionizeSyncService sessionizeSyncService,
        ILogger<Program> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Processing ConferenceUpdated event for conference {ConferenceId}", @event.ConferenceId);

            var result = await sessionizeSyncService.SynchronizeConferenceAsync(@event.ConferenceId, cancellationToken);

            if (result == null)
            {
                logger.LogWarning("Conference {ConferenceId} not found", @event.ConferenceId);
                return Results.BadRequest(new { error = "Conference not found" });
            }

            return Results.Ok(new
            {
                message = "Conference synchronized successfully",
                conferenceId = result.ConferenceId,
                speakersCount = result.SpeakersCount,
                roomsCount = result.RoomsCount,
                presentationsCount = result.PresentationsCount
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing ConferenceUpdated event for conference {ConferenceId}", @event.ConferenceId);
            return Results.BadRequest(new { error = "Failed to process conference updated event", details = ex.Message });
        }
    }
}
