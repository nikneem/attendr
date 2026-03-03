using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Api.Authorization;
using HexMaster.Attendr.Conferences.Features.ManageSpeakers;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Profiles.Integrations.Services;
using System.Security.Claims;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

public static class ConferenceSpeakersEndpoints
{
    public static IEndpointRouteBuilder MapConferenceSpeakersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conferences/{conferenceId:guid}/speakers")
            .WithName("ConferenceSpeakers")
            .RequireAuthorization();

        group.MapGet("/", ListSpeakers)
            .WithName("ListConferenceSpeakers")
            .Produces<List<ConferenceSpeakerDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateSpeaker)
            .WithName("CreateConferenceSpeaker")
            .Produces<ConferenceSpeakerDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{speakerId:guid}", UpdateSpeaker)
            .WithName("UpdateConferenceSpeaker")
            .Produces<ConferenceSpeakerDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{speakerId:guid}", DeleteSpeaker)
            .WithName("DeleteConferenceSpeaker")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListSpeakers(
        Guid conferenceId,
        IQueryHandler<ListConferenceSpeakersQuery, List<ConferenceSpeakerDto>> handler,
        IProfilesIntegrationService profilesIntegration,
        IConferenceRepository conferenceRepository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var authResult = await ConferenceAuthorizationHelper.AuthorizeConferenceAccessAsync(conferenceId, user, profilesIntegration, conferenceRepository, cancellationToken);
        if (authResult is not null) return authResult;

        try
        {
            var result = await handler.Handle(new ListConferenceSpeakersQuery(conferenceId), cancellationToken);
            return Results.Ok(result);
        }
        catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    }

    private static async Task<IResult> CreateSpeaker(
        Guid conferenceId,
        CreateConferenceSpeakerRequest request,
        ICommandHandler<CreateConferenceSpeakerCommand, ConferenceSpeakerDto> handler,
        IProfilesIntegrationService profilesIntegration,
        IConferenceRepository conferenceRepository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var authResult = await ConferenceAuthorizationHelper.AuthorizeConferenceAccessAsync(conferenceId, user, profilesIntegration, conferenceRepository, cancellationToken);
        if (authResult is not null) return authResult;

        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest(new { error = "Name is required" });

        try
        {
            var command = new CreateConferenceSpeakerCommand(conferenceId, request.Name, request.Company, request.ProfilePictureUrl);
            var result = await handler.Handle(command, cancellationToken);
            return Results.Created($"/api/conferences/{conferenceId}/speakers/{result.Id}", result);
        }
        catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
        catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    }

    private static async Task<IResult> UpdateSpeaker(
        Guid conferenceId,
        Guid speakerId,
        UpdateConferenceSpeakerRequest request,
        ICommandHandler<UpdateConferenceSpeakerCommand, ConferenceSpeakerDto> handler,
        IProfilesIntegrationService profilesIntegration,
        IConferenceRepository conferenceRepository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var authResult = await ConferenceAuthorizationHelper.AuthorizeConferenceAccessAsync(conferenceId, user, profilesIntegration, conferenceRepository, cancellationToken);
        if (authResult is not null) return authResult;

        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest(new { error = "Name is required" });

        try
        {
            var command = new UpdateConferenceSpeakerCommand(conferenceId, speakerId, request.Name, request.Company, request.ProfilePictureUrl);
            var result = await handler.Handle(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
        catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    }

    private static async Task<IResult> DeleteSpeaker(
        Guid conferenceId,
        Guid speakerId,
        ICommandHandler<DeleteConferenceSpeakerCommand, bool> handler,
        IProfilesIntegrationService profilesIntegration,
        IConferenceRepository conferenceRepository,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var authResult = await ConferenceAuthorizationHelper.AuthorizeConferenceAccessAsync(conferenceId, user, profilesIntegration, conferenceRepository, cancellationToken);
        if (authResult is not null) return authResult;

        try
        {
            await handler.Handle(new DeleteConferenceSpeakerCommand(conferenceId, speakerId), cancellationToken);
            return Results.NoContent();
        }
        catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
    }
}
