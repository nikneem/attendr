using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.CreateConference;
using HexMaster.Attendr.Conferences.GetConference;
using HexMaster.Attendr.Conferences.ListConferences;
using HexMaster.Attendr.Conferences.UpdateConference;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.IntegrationEvents.Events;
using HexMaster.Attendr.IntegrationEvents.Services;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

public static class ConferencesEndpoints
{
    public static IEndpointRouteBuilder MapConferencesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conferences")
            .WithName("Conferences");

        group.MapGet("/", ListConferences)
            .WithName("ListConferences")
            .Produces<ListConferencesResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapGet("/{id:guid}", GetConference)
            .WithName("GetConference")
            .Produces<ConferenceDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapPost("/", CreateConference)
            .WithName("CreateConference")
            .Produces<CreateConferenceResult>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapPut("/{id:guid}", UpdateConference)
            .WithName("UpdateConference")
            .Produces<ConferenceDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetConference(
        Guid id,
        IQueryHandler<GetConferenceQuery, ConferenceDetailsDto?> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetConferenceQuery(id);
        var result = await handler.Handle(query, cancellationToken);

        if (result == null)
        {
            return Results.NotFound(new { error = "Conference not found" });
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> ListConferences(
        IQueryHandler<ListConferencesQuery, ListConferencesResult> handler,
        string? search = null,
        int? pageSize = null,
        int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        var query = new ListConferencesQuery(search, pageSize, pageNumber);
        var result = await handler.Handle(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateConference(
        CreateConferenceRequest request,
        ICommandHandler<CreateConferenceCommand, CreateConferenceResult> handler,
        IIntegrationEventPublisher eventPublisher,
        CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Results.BadRequest(new { error = "Title is required" });
        }

        if (request.EndDate < request.StartDate)
        {
            return Results.BadRequest(new { error = "End date cannot be before start date" });
        }

        try
        {
            // Create command
            var command = new CreateConferenceCommand(
                request.Title.Trim(),
                request.City?.Trim() ?? "Unknown",
                request.Country?.Trim() ?? "Unknown",
                request.ImageUrl?.Trim(),
                request.StartDate,
                request.EndDate,
                request.SynchronizationSource);

            // Execute command
            var result = await handler.Handle(command, cancellationToken);

            // Publish integration event
            var conferenceCreatedEvent = new ConferenceCreatedEvent
            {
                ConferenceId = result.Id,
                Title = result.Title,
                City = request.City?.Trim() ?? "Unknown",
                Country = request.Country?.Trim() ?? "Unknown",
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };
            await eventPublisher.PublishAsync(conferenceCreatedEvent, cancellationToken);

            return Results.Created($"/api/conferences/{result.Id}", result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> UpdateConference(
        Guid id,
        CreateConferenceRequest request,
        ICommandHandler<UpdateConferenceCommand, ConferenceDetailsDto> handler,
        IIntegrationEventPublisher eventPublisher,
        CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Results.BadRequest(new { error = "Title is required" });
        }

        if (request.EndDate < request.StartDate)
        {
            return Results.BadRequest(new { error = "End date cannot be before start date" });
        }

        try
        {
            // Create command
            var command = new UpdateConferenceCommand(
                id,
                request.Title.Trim(),
                request.City?.Trim() ?? "Unknown",
                request.Country?.Trim() ?? "Unknown",
                request.ImageUrl?.Trim(),
                request.StartDate,
                request.EndDate,
                request.SynchronizationSource);

            // Execute command
            var result = await handler.Handle(command, cancellationToken);

            // Publish integration event
            var conferenceUpdatedEvent = new ConferenceUpdatedEvent
            {
                ConferenceId = result.Id,
                Title = result.Title,
                City = result.City ?? "Unknown",
                Country = result.Country ?? "Unknown",
                StartDate = result.StartDate,
                EndDate = result.EndDate
            };
            await eventPublisher.PublishAsync(conferenceUpdatedEvent, cancellationToken);

            return Results.Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound(new { error = "Conference not found" });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
