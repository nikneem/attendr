using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Api.Authorization;
using HexMaster.Attendr.Conferences.Features.Rooms.CreateRoom;
using HexMaster.Attendr.Conferences.Features.Rooms.DeleteRoom;
using HexMaster.Attendr.Conferences.Features.Rooms.GetRoom;
using HexMaster.Attendr.Conferences.Features.Rooms.ListRooms;
using HexMaster.Attendr.Conferences.Features.Rooms.UpdateRoom;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Profiles.Integrations.Services;
using System.Security.Claims;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

public static class RoomsEndpoints
{
    public static IEndpointRouteBuilder MapRoomsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conferences/{conferenceId:guid}/rooms")
            .WithName("Rooms")
            .RequireAuthorization();

        group.MapGet("/", ListRooms)
            .WithName("ListRooms")
            .Produces<IReadOnlyList<ConferenceRoomDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{roomId:guid}", GetRoom)
            .WithName("GetRoom")
            .Produces<ConferenceRoomDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateRoom)
            .WithName("CreateRoom")
            .Produces<ConferenceRoomDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{roomId:guid}", UpdateRoom)
            .WithName("UpdateRoom")
            .Produces<ConferenceRoomDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{roomId:guid}", DeleteRoom)
            .WithName("DeleteRoom")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListRooms(
        Guid conferenceId,
        IQueryHandler<ListRoomsQuery, IReadOnlyList<ConferenceRoomDto>> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.Handle(new ListRoomsQuery(conferenceId), cancellationToken);
            return Results.Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetRoom(
        Guid conferenceId,
        Guid roomId,
        IQueryHandler<GetRoomQuery, ConferenceRoomDto?> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.Handle(new GetRoomQuery(conferenceId, roomId), cancellationToken);
            if (result == null) return Results.NotFound(new { error = "Room not found" });
            return Results.Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> CreateRoom(
        Guid conferenceId,
        CreateConferenceRoomRequest request,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<CreateRoomCommand, ConferenceRoomDto> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest(new { error = "Name is required" });
        if (request.Capacity <= 0)
            return Results.BadRequest(new { error = "Capacity must be greater than 0" });

        try
        {
            var (profileId, isAdmin) = await ResolveProfileAsync(profilesIntegration, user, cancellationToken);
            var command = new CreateRoomCommand(conferenceId, request.Name.Trim(), request.Capacity, profileId, isAdmin);
            var result = await handler.Handle(command, cancellationToken);
            return Results.Created($"/api/conferences/{conferenceId}/rooms/{result.Id}", result);
        }
        catch (ForbiddenException ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status403Forbidden);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
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

    private static async Task<IResult> UpdateRoom(
        Guid conferenceId,
        Guid roomId,
        UpdateConferenceRoomRequest request,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<UpdateRoomCommand, ConferenceRoomDto> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest(new { error = "Name is required" });
        if (request.Capacity <= 0)
            return Results.BadRequest(new { error = "Capacity must be greater than 0" });

        try
        {
            var (profileId, isAdmin) = await ResolveProfileAsync(profilesIntegration, user, cancellationToken);
            var command = new UpdateRoomCommand(conferenceId, roomId, request.Name.Trim(), request.Capacity, profileId, isAdmin);
            var result = await handler.Handle(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (ForbiddenException ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status403Forbidden);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
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

    private static async Task<IResult> DeleteRoom(
        Guid conferenceId,
        Guid roomId,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<DeleteRoomCommand, bool> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        try
        {
            var (profileId, isAdmin) = await ResolveProfileAsync(profilesIntegration, user, cancellationToken);
            var command = new DeleteRoomCommand(conferenceId, roomId, profileId, isAdmin);
            var deleted = await handler.Handle(command, cancellationToken);
            if (!deleted) return Results.NotFound(new { error = "Room not found" });
            return Results.NoContent();
        }
        catch (ForbiddenException ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status403Forbidden);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static Task<(Guid? profileId, bool isAdmin)> ResolveProfileAsync(
        IProfilesIntegrationService profilesIntegration,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
        => EndpointHelpers.ResolveProfileAsync(profilesIntegration, user, cancellationToken);

    private static bool IsAdminUser(ClaimsPrincipal user)
        => EndpointHelpers.IsAdminUser(user);
}
