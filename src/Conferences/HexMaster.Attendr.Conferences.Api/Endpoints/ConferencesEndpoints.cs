using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.CreateConference;
using HexMaster.Attendr.Conferences.ListConferences;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.QueryHandlers;

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

        group.MapPost("/", CreateConference)
            .WithName("CreateConference")
            .Produces<CreateConferenceResult>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> ListConferences(
        IQueryHandler<ListConferencesQuery, ListConferencesResult> handler,
        string? search = null,
        int? pageSize = null,
        int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        var query = new ListConferencesQuery(search, pageSize, pageNumber);
        var result = await handler.HandleAsync(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateConference(
        CreateConferenceRequest request,
        ICommandHandler<CreateConferenceCommand, CreateConferenceResult> handler,
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
                request.StartDate,
                request.EndDate,
                request.SynchronizationSource);

            // Execute command
            var result = await handler.Handle(command, cancellationToken);

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
}
