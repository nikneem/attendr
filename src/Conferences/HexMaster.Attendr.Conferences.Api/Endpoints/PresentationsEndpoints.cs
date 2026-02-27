using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Api.Authorization;
using HexMaster.Attendr.Conferences.Features.Presentations.CreatePresentation;
using HexMaster.Attendr.Conferences.Features.Presentations.DeletePresentation;
using HexMaster.Attendr.Conferences.Features.Presentations.GetPresentation;
using HexMaster.Attendr.Conferences.Features.Presentations.ListPresentations;
using HexMaster.Attendr.Conferences.Features.Presentations.UpdatePresentation;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Services;
using System.Security.Claims;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

public static class PresentationsEndpoints
{
    public static IEndpointRouteBuilder MapPresentationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conferences/{conferenceId:guid}/presentations")
            .WithName("Presentations")
            .RequireAuthorization();

        group.MapGet("/", ListPresentations)
            .WithName("ListPresentations")
            .Produces<IReadOnlyList<ConferencePresentationDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{presentationId:guid}", GetPresentation)
            .WithName("GetPresentation")
            .Produces<ConferencePresentationDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreatePresentation)
            .WithName("CreatePresentation")
            .Produces<ConferencePresentationDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{presentationId:guid}", UpdatePresentation)
            .WithName("UpdatePresentation")
            .Produces<ConferencePresentationDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{presentationId:guid}", DeletePresentation)
            .WithName("DeletePresentation")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListPresentations(
        Guid conferenceId,
        IQueryHandler<ListPresentationsQuery, IReadOnlyList<ConferencePresentationDto>> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.Handle(new ListPresentationsQuery(conferenceId), cancellationToken);
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

    private static async Task<IResult> GetPresentation(
        Guid conferenceId,
        Guid presentationId,
        IQueryHandler<GetPresentationQuery, ConferencePresentationDto?> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.Handle(new GetPresentationQuery(conferenceId, presentationId), cancellationToken);
            if (result == null) return Results.NotFound(new { error = "Presentation not found" });
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

    private static async Task<IResult> CreatePresentation(
        Guid conferenceId,
        CreateConferencePresentationRequest request,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<CreatePresentationCommand, ConferencePresentationDto> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Results.BadRequest(new { error = "Title is required" });
        if (string.IsNullOrWhiteSpace(request.Abstract))
            return Results.BadRequest(new { error = "Abstract is required" });
        if (request.SpeakerIds == null || request.SpeakerIds.Count == 0)
            return Results.BadRequest(new { error = "At least one speaker is required" });
        if (request.EndDateTime <= request.StartDateTime)
            return Results.BadRequest(new { error = "End date/time must be after start date/time" });

        try
        {
            var (profileId, isAdmin) = await ResolveProfileAsync(profilesIntegration, user, cancellationToken);
            var command = new CreatePresentationCommand(
                conferenceId, request.Title.Trim(), request.Abstract.Trim(),
                request.StartDateTime, request.EndDateTime, request.RoomId, request.SpeakerIds,
                profileId, isAdmin);
            var result = await handler.Handle(command, cancellationToken);
            return Results.Created($"/api/conferences/{conferenceId}/presentations/{result.Id}", result);
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

    private static async Task<IResult> UpdatePresentation(
        Guid conferenceId,
        Guid presentationId,
        UpdateConferencePresentationRequest request,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<UpdatePresentationCommand, ConferencePresentationDto> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Results.BadRequest(new { error = "Title is required" });
        if (string.IsNullOrWhiteSpace(request.Abstract))
            return Results.BadRequest(new { error = "Abstract is required" });
        if (request.SpeakerIds == null || request.SpeakerIds.Count == 0)
            return Results.BadRequest(new { error = "At least one speaker is required" });
        if (request.EndDateTime <= request.StartDateTime)
            return Results.BadRequest(new { error = "End date/time must be after start date/time" });

        try
        {
            var (profileId, isAdmin) = await ResolveProfileAsync(profilesIntegration, user, cancellationToken);
            var command = new UpdatePresentationCommand(
                conferenceId, presentationId, request.Title.Trim(), request.Abstract.Trim(),
                request.StartDateTime, request.EndDateTime, request.RoomId, request.SpeakerIds,
                profileId, isAdmin);
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

    private static async Task<IResult> DeletePresentation(
        Guid conferenceId,
        Guid presentationId,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<DeletePresentationCommand, bool> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        try
        {
            var (profileId, isAdmin) = await ResolveProfileAsync(profilesIntegration, user, cancellationToken);
            var command = new DeletePresentationCommand(conferenceId, presentationId, profileId, isAdmin);
            var deleted = await handler.Handle(command, cancellationToken);
            if (!deleted) return Results.NotFound(new { error = "Presentation not found" });
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
        catch (Exception)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<(Guid? profileId, bool isAdmin)> ResolveProfileAsync(
        IProfilesIntegrationService profilesIntegration,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var isAdmin = IsAdminUser(user);
        Guid? profileId = null;
        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(user, cancellationToken);
            if (Guid.TryParse(profile.ProfileId, out var id))
                profileId = id;
        }
        catch { }
        return (profileId, isAdmin);
    }

    private static bool IsAdminUser(ClaimsPrincipal user)
    {
        var permissionsClaim = user.FindFirst("permissions");
        if (permissionsClaim == null) return false;
        var permissions = permissionsClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return permissions.Contains(Permissions.AdminAttendr);
    }
}
