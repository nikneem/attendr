using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Api.Authorization;
using HexMaster.Attendr.Conferences.Features.Speakers.CreateSpeaker;
using HexMaster.Attendr.Conferences.Features.Speakers.DeleteSpeaker;
using HexMaster.Attendr.Conferences.Features.Speakers.GetSpeaker;
using HexMaster.Attendr.Conferences.Features.Speakers.ListSpeakers;
using HexMaster.Attendr.Conferences.Features.Speakers.UpdateSpeaker;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Profiles.Integrations.Services;
using System.Security.Claims;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

public static class SpeakersEndpoints
{
    public static IEndpointRouteBuilder MapSpeakersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conferences/{conferenceId:guid}/speakers")
            .WithName("Speakers")
            .RequireAuthorization();

        group.MapGet("/", ListSpeakers)
            .WithName("ListSpeakers")
            .Produces<IReadOnlyList<ConferenceSpeakerDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{speakerId:guid}", GetSpeaker)
            .WithName("GetSpeaker")
            .Produces<ConferenceSpeakerDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateSpeaker)
            .WithName("CreateSpeaker")
            .Produces<ConferenceSpeakerDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{speakerId:guid}", UpdateSpeaker)
            .WithName("UpdateSpeaker")
            .Produces<ConferenceSpeakerDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{speakerId:guid}", DeleteSpeaker)
            .WithName("DeleteSpeaker")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListSpeakers(
        Guid conferenceId,
        IQueryHandler<ListSpeakersQuery, IReadOnlyList<ConferenceSpeakerDto>> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.Handle(new ListSpeakersQuery(conferenceId), cancellationToken);
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

    private static async Task<IResult> GetSpeaker(
        Guid conferenceId,
        Guid speakerId,
        IQueryHandler<GetSpeakerQuery, ConferenceSpeakerDto?> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.Handle(new GetSpeakerQuery(conferenceId, speakerId), cancellationToken);
            if (result == null) return Results.NotFound(new { error = "Speaker not found" });
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

    private static async Task<IResult> CreateSpeaker(
        Guid conferenceId,
        CreateConferenceSpeakerRequest request,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<CreateSpeakerCommand, ConferenceSpeakerDto> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest(new { error = "Name is required" });

        try
        {
            var (profileId, isAdmin) = await ResolveProfileAsync(profilesIntegration, user, cancellationToken);
            var command = new CreateSpeakerCommand(conferenceId, request.Name.Trim(), request.Company?.Trim(), request.ProfilePictureUrl?.Trim(), profileId, isAdmin);
            var result = await handler.Handle(command, cancellationToken);
            return Results.Created($"/api/conferences/{conferenceId}/speakers/{result.Id}", result);
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

    private static async Task<IResult> UpdateSpeaker(
        Guid conferenceId,
        Guid speakerId,
        UpdateConferenceSpeakerRequest request,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<UpdateSpeakerCommand, ConferenceSpeakerDto> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest(new { error = "Name is required" });

        try
        {
            var (profileId, isAdmin) = await ResolveProfileAsync(profilesIntegration, user, cancellationToken);
            var command = new UpdateSpeakerCommand(conferenceId, speakerId, request.Name.Trim(), request.Company?.Trim(), request.ProfilePictureUrl?.Trim(), profileId, isAdmin);
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

    private static async Task<IResult> DeleteSpeaker(
        Guid conferenceId,
        Guid speakerId,
        IProfilesIntegrationService profilesIntegration,
        ICommandHandler<DeleteSpeakerCommand, bool> handler,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        try
        {
            var (profileId, isAdmin) = await ResolveProfileAsync(profilesIntegration, user, cancellationToken);
            var command = new DeleteSpeakerCommand(conferenceId, speakerId, profileId, isAdmin);
            var deleted = await handler.Handle(command, cancellationToken);
            if (!deleted) return Results.NotFound(new { error = "Speaker not found" });
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

    private static Task<(Guid? profileId, bool isAdmin)> ResolveProfileAsync(
        IProfilesIntegrationService profilesIntegration,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
        => EndpointHelpers.ResolveProfileAsync(profilesIntegration, user, cancellationToken);

    private static bool IsAdminUser(ClaimsPrincipal user)
        => EndpointHelpers.IsAdminUser(user);
}
