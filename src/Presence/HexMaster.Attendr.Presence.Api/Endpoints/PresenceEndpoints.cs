using System.Security.Claims;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Presence.Abstractions.Dtos;
using HexMaster.Attendr.Presence.Features.CheckIn;
using HexMaster.Attendr.Presence.Features.GetConferenceAttendance;
using HexMaster.Attendr.Presence.Features.GetConferenceScheduleNow;
using HexMaster.Attendr.Presence.Features.GetConferenceWithPresentations;
using HexMaster.Attendr.Presence.Features.GetMyConferences;
using HexMaster.Attendr.Presence.Features.GetCurrentConferences;
using HexMaster.Attendr.Presence.Features.RatePresentation;
using HexMaster.Attendr.Presence.Features.SetPreferredPresentation;
using HexMaster.Attendr.Presence.Features.UnfollowConference;
using HexMaster.Attendr.Presence.Features.UpdateAttendance;
using HexMaster.Attendr.Profiles.Integrations.Extensions;
using HexMaster.Attendr.Profiles.Integrations.Services;
using Microsoft.AspNetCore.Mvc;

namespace HexMaster.Attendr.Presence.Api.Endpoints;

/// <summary>
/// Extension methods to map presence endpoints.
/// </summary>
public static class PresenceEndpoints
{
    /// <summary>
    /// Maps the presence endpoints to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static IEndpointRouteBuilder MapPresenceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/presence")
            .WithName("Presence")
            .RequireAuthorization();

        group.MapGet("/my-conferences", GetMyConferences)
            .WithName("GetMyConferences")
            .Produces<List<MyConferenceResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/now", GetCurrentConferences)
            .WithName("GetCurrentConferences")
            .Produces<List<CurrentConferenceResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{conferenceId:guid}/attendance", UpdateAttendance)
            .WithName("UpdateAttendance")
            .Accepts<UpdateAttendanceDto>("application/json")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{conferenceId:guid}", UnfollowConference)
            .WithName("UnfollowConference")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{conferenceId:guid}/attendance", GetConferenceAttendance)
            .WithName("GetConferenceAttendance")
            .Produces<ConferenceAttendanceDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/{conferenceId:guid}", GetConferenceWithPresentations)
            .WithName("GetConferenceWithPresentations")
            .Produces<ConferenceWithPresentationsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{conferenceId:guid}/rate", GetPresentationToRate)
            .WithName("GetPresentationToRate")
            .Produces<PresentationToRateDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{conferenceId:guid}/rate/{presentationId:guid}", RatePresentation)
            .WithName("RatePresentation")
            .Accepts<RatePresentationDto>("application/json")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{conferenceId:guid}/checkin/{presentationId:guid}", CheckIn)
            .WithName("CheckIn")
            .Accepts<CheckInRequest>("application/json")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{conferenceId:guid}/prefer/{presentationId:guid}", SetPreferredPresentation)
            .WithName("SetPreferredPresentation")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{conferenceId:guid}/now", GetConferenceScheduleNow)
            .WithName("GetConferenceScheduleNow")
            .Produces<ConferenceScheduleNowResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetMyConferences(
        HttpContext context,
        IQueryHandler<GetMyConferencesQuery, List<MyConferenceResponse>> handler,
        IProfilesIntegrationService profilesIntegration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("GetMyConferencesEndpoint");

        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(context.User, cancellationToken);
            if (!Guid.TryParse(profile.ProfileId, out var profileId))
            {
                logger.LogError("Invalid profile ID format: {ProfileId}", profile.ProfileId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }

            var result = await handler.Handle(new GetMyConferencesQuery(profileId), cancellationToken);
            return Results.Ok(result);
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (ProfileNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving conferences for user");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetCurrentConferences(
        HttpContext context,
        IQueryHandler<GetCurrentConferencesQuery, List<CurrentConferenceResponse>> handler,
        IProfilesIntegrationService profilesIntegration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("GetCurrentConferencesEndpoint");

        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(context.User, cancellationToken);
            if (!Guid.TryParse(profile.ProfileId, out var profileId))
            {
                logger.LogError("Invalid profile ID format: {ProfileId}", profile.ProfileId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }

            var result = await handler.Handle(new GetCurrentConferencesQuery(profileId), cancellationToken);
            return Results.Ok(result);
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (ProfileNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving current conferences for user");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetPresentationToRate(
        Guid conferenceId,
        int index,
        HttpContext context,
        IQueryHandler<GetRandomPresentationToRateQuery, PresentationToRateDto?> handler,
        IProfilesIntegrationService profilesIntegration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("GetPresentationToRateEndpoint");

        // Validate index parameter
        if (index < 0 || index > 2)
        {
            return Results.BadRequest(new { error = "Index must be between 0 and 2" });
        }

        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(context.User, cancellationToken);
            if (!Guid.TryParse(profile.ProfileId, out var profileId))
            {
                logger.LogError("Invalid profile ID format: {ProfileId}", profile.ProfileId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }

            var result = await handler.Handle(
                new GetRandomPresentationToRateQuery(profileId, conferenceId, index),
                cancellationToken);

            if (result == null)
            {
                return Results.NotFound(new { error = "No presentation found at the requested index" });
            }

            return Results.Ok(result);
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (ProfileNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting presentation to rate for conference {ConferenceId} at index {Index}", conferenceId, index);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> RatePresentation(
        Guid conferenceId,
        Guid presentationId,
        RatePresentationDto ratingDto,
        HttpContext context,
        ICommandHandler<RatePresentationCommand> handler,
        IProfilesIntegrationService profilesIntegration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("RatePresentationEndpoint");

        try
        {
            // Validate rating value
            if (ratingDto.Rating.HasValue && ratingDto.Rating.Value > 5)
            {
                return Results.BadRequest(new { error = "Rating must be between 0 and 5." });
            }

            var profile = await profilesIntegration.GetProfileFromUser(context.User, cancellationToken);
            if (!Guid.TryParse(profile.ProfileId, out var profileId))
            {
                logger.LogError("Invalid profile ID format: {ProfileId}", profile.ProfileId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }

            await handler.Handle(
                new RatePresentationCommand(profileId, conferenceId, presentationId, ratingDto),
                cancellationToken);

            return Results.Accepted();
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (ProfileNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Failed to rate presentation {PresentationId}", presentationId);
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rating presentation {PresentationId} for conference {ConferenceId}", presentationId, conferenceId);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> UpdateAttendance(
        Guid conferenceId,
        UpdateAttendanceDto requestDto,
        HttpContext context,
        ICommandHandler<UpdateAttendanceCommand> handler,
        IProfilesIntegrationService profilesIntegration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("UpdateAttendanceEndpoint");

        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(context.User, cancellationToken);
            if (!Guid.TryParse(profile.ProfileId, out var profileId))
            {
                logger.LogError("Invalid profile ID format: {ProfileId}", profile.ProfileId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }

            await handler.Handle(
                new UpdateAttendanceCommand(conferenceId, profileId, requestDto.IsAttending),
                cancellationToken);

            return Results.Accepted();
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (ProfileNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Failed to update attendance for conference {ConferenceId}", conferenceId);
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating attendance for conference {ConferenceId}", conferenceId);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> UnfollowConference(
        Guid conferenceId,
        HttpContext context,
        ICommandHandler<UnfollowConferenceCommand> handler,
        IProfilesIntegrationService profilesIntegration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("UnfollowConferenceEndpoint");

        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(context.User, cancellationToken);
            if (!Guid.TryParse(profile.ProfileId, out var profileId))
            {
                logger.LogError("Invalid profile ID format: {ProfileId}", profile.ProfileId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }

            await handler.Handle(
                new UnfollowConferenceCommand(conferenceId, profileId),
                cancellationToken);

            return Results.NoContent();
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (ProfileNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Failed to unfollow conference {ConferenceId}", conferenceId);
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unfollowing conference {ConferenceId}", conferenceId);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetConferenceAttendance(
        Guid conferenceId,
        HttpContext context,
        IQueryHandler<GetConferenceAttendanceQuery, ConferenceAttendanceDto> handler,
        IProfilesIntegrationService profilesIntegration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("GetConferenceAttendanceEndpoint");

        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(context.User, cancellationToken);
            if (!Guid.TryParse(profile.ProfileId, out var profileId))
            {
                logger.LogError("Invalid profile ID format: {ProfileId}", profile.ProfileId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }

            var result = await handler.Handle(
                new GetConferenceAttendanceQuery(profileId, conferenceId),
                cancellationToken);

            return Results.Ok(result);
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (ProfileNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting conference attendance for conference {ConferenceId}", conferenceId);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetConferenceWithPresentations(
        Guid conferenceId,
        HttpContext context,
        IQueryHandler<GetConferenceWithPresentationsQuery, ConferenceWithPresentationsResponse> handler,
        IProfilesIntegrationService profilesIntegration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("GetConferenceWithPresentationsEndpoint");

        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(context.User, cancellationToken);
            if (!Guid.TryParse(profile.ProfileId, out var profileId))
            {
                logger.LogError("Invalid profile ID format: {ProfileId}", profile.ProfileId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }

            var result = await handler.Handle(
                new GetConferenceWithPresentationsQuery(profileId, conferenceId),
                cancellationToken);

            return Results.Ok(result);
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (ProfileNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Conference presence not found for conference {ConferenceId}", conferenceId);
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting conference with presentations for conference {ConferenceId}", conferenceId);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> CheckIn(
        Guid conferenceId,
        Guid presentationId,
        CheckInRequest request,
        HttpContext context,
        ICommandHandler<CheckInCommand> handler,
        IProfilesIntegrationService profilesIntegration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("CheckInEndpoint");

        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(context.User, cancellationToken);
            if (!Guid.TryParse(profile.ProfileId, out var profileId))
            {
                logger.LogError("Invalid profile ID format: {ProfileId}", profile.ProfileId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }

            await handler.Handle(
                new CheckInCommand(profileId, conferenceId, presentationId, request.IsCheckedIn),
                cancellationToken);

            return Results.Accepted();
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (ProfileNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Failed to check in to presentation {PresentationId}", presentationId);
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking in to presentation {PresentationId} for conference {ConferenceId}",
                presentationId, conferenceId);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> SetPreferredPresentation(
        Guid conferenceId,
        Guid presentationId,
        HttpContext context,
        ICommandHandler<SetPreferredPresentationCommand> handler,
        IProfilesIntegrationService profilesIntegration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("SetPreferredPresentationEndpoint");

        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(context.User, cancellationToken);
            if (!Guid.TryParse(profile.ProfileId, out var profileId))
            {
                logger.LogError("Invalid profile ID format: {ProfileId}", profile.ProfileId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }

            await handler.Handle(
                new SetPreferredPresentationCommand(profileId, conferenceId, presentationId),
                cancellationToken);

            return Results.Accepted();
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (ProfileNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Failed to set preferred presentation {PresentationId}", presentationId);
            return Results.NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Cannot set presentation {PresentationId} as preferred", presentationId);
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting preferred presentation {PresentationId} for conference {ConferenceId}",
                presentationId, conferenceId);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> GetConferenceScheduleNow(
        Guid conferenceId,
        HttpContext context,
        IQueryHandler<GetConferenceScheduleNowQuery, ConferenceScheduleNowResponse> handler,
        IProfilesIntegrationService profilesIntegration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("GetConferenceScheduleNowEndpoint");

        try
        {
            var profile = await profilesIntegration.GetProfileFromUser(context.User, cancellationToken);
            if (!Guid.TryParse(profile.ProfileId, out var profileId))
            {
                logger.LogError("Invalid profile ID format: {ProfileId}", profile.ProfileId);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }

            var result = await handler.Handle(
                new GetConferenceScheduleNowQuery(profileId, conferenceId),
                cancellationToken);

            return Results.Ok(result);
        }
        catch (UnauthorizedException)
        {
            return Results.Unauthorized();
        }
        catch (ProfileNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting conference schedule now for conference {ConferenceId}", conferenceId);
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
