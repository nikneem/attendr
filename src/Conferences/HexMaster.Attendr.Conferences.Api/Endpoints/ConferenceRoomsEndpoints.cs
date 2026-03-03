using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Api.Authorization;
using HexMaster.Attendr.Conferences.Features.ManageRooms;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Services;
using System.Security.Claims;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

public static class ConferenceRoomsEndpoints
{
    public static IEndpointRouteBuilder MapConferenceRoomsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conferences/{conferenceId:guid}/rooms")
            .WithName("ConferenceRooms")
            .RequireAuthorization();

        group.MapGet("/", ListRooms)
            .WithName("ListConferenceRooms")
            .Produces<List<ConferenceRoomDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateRoom)
            .WithName("CreateConferenceRoom")
            .Produces<ConferenceRoomDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{roomId:guid}", UpdateRoom)
            .WithName("UpdateConferenceRoom")
            .Produces<ConferenceRoomDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{roomId:guid}", DeleteRoom)
            .WithName("DeleteConferenceRoom")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListRooms(
        Guid conferenceId,
        IQueryHandler<ListConferenceRoomsQuery, List<ConferenceRoomDto>> handler,
        IProfilesIntegrationService profilesIntegration,
        IConferenceRepository conferenceRepository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var authResult = await AuthorizeAsync(conferenceId, user, profilesIntegration, conferenceRepository, cancellationToken);
        if (authResult is not null) return authResult;

        try
        {
            var result = await handler.Handle(new ListConferenceRoomsQuery(conferenceId), cancellationToken);
            return Results.Ok(result);
        }
        catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    }

    private static async Task<IResult> CreateRoom(
        Guid conferenceId,
        CreateConferenceRoomRequest request,
        ICommandHandler<CreateConferenceRoomCommand, ConferenceRoomDto> handler,
        IProfilesIntegrationService profilesIntegration,
        IConferenceRepository conferenceRepository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var authResult = await AuthorizeAsync(conferenceId, user, profilesIntegration, conferenceRepository, cancellationToken);
        if (authResult is not null) return authResult;

        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest(new { error = "Name is required" });

        if (request.Capacity <= 0)
            return Results.BadRequest(new { error = "Capacity must be greater than zero" });

        try
        {
            var command = new CreateConferenceRoomCommand(conferenceId, request.Name, request.Capacity);
            var result = await handler.Handle(command, cancellationToken);
            return Results.Created($"/api/conferences/{conferenceId}/rooms/{result.Id}", result);
        }
        catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
        catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    }

    private static async Task<IResult> UpdateRoom(
        Guid conferenceId,
        Guid roomId,
        UpdateConferenceRoomRequest request,
        ICommandHandler<UpdateConferenceRoomCommand, ConferenceRoomDto> handler,
        IProfilesIntegrationService profilesIntegration,
        IConferenceRepository conferenceRepository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var authResult = await AuthorizeAsync(conferenceId, user, profilesIntegration, conferenceRepository, cancellationToken);
        if (authResult is not null) return authResult;

        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest(new { error = "Name is required" });

        if (request.Capacity <= 0)
            return Results.BadRequest(new { error = "Capacity must be greater than zero" });

        try
        {
            var command = new UpdateConferenceRoomCommand(conferenceId, roomId, request.Name, request.Capacity);
            var result = await handler.Handle(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
        catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    }

    private static async Task<IResult> DeleteRoom(
        Guid conferenceId,
        Guid roomId,
        ICommandHandler<DeleteConferenceRoomCommand, bool> handler,
        IProfilesIntegrationService profilesIntegration,
        IConferenceRepository conferenceRepository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var authResult = await AuthorizeAsync(conferenceId, user, profilesIntegration, conferenceRepository, cancellationToken);
        if (authResult is not null) return authResult;

        try
        {
            await handler.Handle(new DeleteConferenceRoomCommand(conferenceId, roomId), cancellationToken);
            return Results.NoContent();
        }
        catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
    }

    private static async Task<IResult?> AuthorizeAsync(
        Guid conferenceId,
        ClaimsPrincipal user,
        IProfilesIntegrationService profilesIntegration,
        IConferenceRepository conferenceRepository,
        CancellationToken cancellationToken)
    {
        if (IsAdminUser(user)) return null;

        Guid? profileId = null;
        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
            if (Guid.TryParse(profile.ProfileId, out var pid)) profileId = pid;
        }
        catch (UnauthorizedException) { return Results.Unauthorized(); }
        catch (ProfileNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }

        if (profileId is null) return Results.Unauthorized();

        var conference = await conferenceRepository.GetByIdAsync(conferenceId, cancellationToken);
        if (conference is null) return Results.NotFound(new { error = "Conference not found" });

        if (conference.CreatedByProfileId != profileId)
            return Results.Forbid();

        return null;
    }

    private static bool IsAdminUser(ClaimsPrincipal user)
    {
        var permissionsClaim = user.FindFirst("permissions");
        if (permissionsClaim == null) return false;
        return permissionsClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(Permissions.AdminAttendr);
    }
}
