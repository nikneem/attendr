using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Api.Authorization;
using HexMaster.Attendr.Conferences.Features.ManagePresentations;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Services;
using System.Security.Claims;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

public static class ConferencePresentationsEndpoints
{
    public static IEndpointRouteBuilder MapConferencePresentationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conferences/{conferenceId:guid}/presentations")
            .WithName("ConferencePresentations")
            .RequireAuthorization();

        group.MapGet("/", ListPresentations)
            .WithName("ListConferencePresentations")
            .Produces<List<ConferencePresentationDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreatePresentation)
            .WithName("CreateConferencePresentation")
            .Produces<ConferencePresentationDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{presentationId:guid}", UpdatePresentation)
            .WithName("UpdateConferencePresentation")
            .Produces<ConferencePresentationDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{presentationId:guid}", DeletePresentation)
            .WithName("DeleteConferencePresentation")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListPresentations(
        Guid conferenceId,
        IQueryHandler<ListConferencePresentationsQuery, List<ConferencePresentationDto>> handler,
        IProfilesIntegrationService profilesIntegration,
        IConferenceRepository conferenceRepository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var authResult = await AuthorizeAsync(conferenceId, user, profilesIntegration, conferenceRepository, cancellationToken);
        if (authResult is not null) return authResult;

        try
        {
            var result = await handler.Handle(new ListConferencePresentationsQuery(conferenceId), cancellationToken);
            return Results.Ok(result);
        }
        catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    }

    private static async Task<IResult> CreatePresentation(
        Guid conferenceId,
        CreateConferencePresentationRequest request,
        ICommandHandler<CreateConferencePresentationCommand, ConferencePresentationDto> handler,
        IProfilesIntegrationService profilesIntegration,
        IConferenceRepository conferenceRepository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var authResult = await AuthorizeAsync(conferenceId, user, profilesIntegration, conferenceRepository, cancellationToken);
        if (authResult is not null) return authResult;

        if (string.IsNullOrWhiteSpace(request.Title))
            return Results.BadRequest(new { error = "Title is required" });

        if (string.IsNullOrWhiteSpace(request.Abstract))
            return Results.BadRequest(new { error = "Abstract is required" });

        if (request.EndDateTime <= request.StartDateTime)
            return Results.BadRequest(new { error = "End date/time must be after start date/time" });

        try
        {
            var command = new CreateConferencePresentationCommand(
                conferenceId, request.Title, request.Abstract,
                request.StartDateTime, request.EndDateTime, request.RoomId, request.SpeakerIds);
            var result = await handler.Handle(command, cancellationToken);
            return Results.Created($"/api/conferences/{conferenceId}/presentations/{result.Id}", result);
        }
        catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
        catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
    }

    private static async Task<IResult> UpdatePresentation(
        Guid conferenceId,
        Guid presentationId,
        UpdateConferencePresentationRequest request,
        ICommandHandler<UpdateConferencePresentationCommand, ConferencePresentationDto> handler,
        IProfilesIntegrationService profilesIntegration,
        IConferenceRepository conferenceRepository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var authResult = await AuthorizeAsync(conferenceId, user, profilesIntegration, conferenceRepository, cancellationToken);
        if (authResult is not null) return authResult;

        if (string.IsNullOrWhiteSpace(request.Title))
            return Results.BadRequest(new { error = "Title is required" });

        if (string.IsNullOrWhiteSpace(request.Abstract))
            return Results.BadRequest(new { error = "Abstract is required" });

        if (request.EndDateTime <= request.StartDateTime)
            return Results.BadRequest(new { error = "End date/time must be after start date/time" });

        try
        {
            var command = new UpdateConferencePresentationCommand(
                conferenceId, presentationId, request.Title, request.Abstract,
                request.StartDateTime, request.EndDateTime, request.RoomId, request.SpeakerIds);
            var result = await handler.Handle(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
        catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
    }

    private static async Task<IResult> DeletePresentation(
        Guid conferenceId,
        Guid presentationId,
        ICommandHandler<DeleteConferencePresentationCommand, bool> handler,
        IProfilesIntegrationService profilesIntegration,
        IConferenceRepository conferenceRepository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var authResult = await AuthorizeAsync(conferenceId, user, profilesIntegration, conferenceRepository, cancellationToken);
        if (authResult is not null) return authResult;

        try
        {
            await handler.Handle(new DeleteConferencePresentationCommand(conferenceId, presentationId), cancellationToken);
            return Results.NoContent();
        }
        catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
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
