using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Features.ManagePresentations;
using HexMaster.Attendr.Core.CommandHandlers;
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
        var authResult = await ConferenceAuthorizationHelper.AuthorizeConferenceAccessAsync(conferenceId, user, profilesIntegration, conferenceRepository, cancellationToken);
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
        var authResult = await ConferenceAuthorizationHelper.AuthorizeConferenceAccessAsync(conferenceId, user, profilesIntegration, conferenceRepository, cancellationToken);
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
        var authResult = await ConferenceAuthorizationHelper.AuthorizeConferenceAccessAsync(conferenceId, user, profilesIntegration, conferenceRepository, cancellationToken);
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
        var authResult = await ConferenceAuthorizationHelper.AuthorizeConferenceAccessAsync(conferenceId, user, profilesIntegration, conferenceRepository, cancellationToken);
        if (authResult is not null) return authResult;

        try
        {
            await handler.Handle(new DeleteConferencePresentationCommand(conferenceId, presentationId), cancellationToken);
            return Results.NoContent();
        }
        catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    }
}
