using System.Security.Claims;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Features.GetProfileTopics;
using HexMaster.Attendr.Profiles.Features.SetTopicManualStatus;

namespace HexMaster.Attendr.Profiles.Api.Endpoints;

public static class ProfileTopicEndpoints
{
    public static IEndpointRouteBuilder MapProfileTopicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profiles/topics")
            .WithName("ProfileTopics");

        group.MapGet("/", GetProfileTopics)
            .WithName("GetProfileTopics")
            .Produces<IReadOnlyList<ProfileTopicDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapPut("/{topicId}", SetTopicManualStatus)
            .WithName("SetTopicManualStatus")
            .Produces<ProfileTopicDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetProfileTopics(
        ClaimsPrincipal user,
        IQueryHandler<GetProfileTopicsQuery, IReadOnlyList<ProfileTopicDto>> handler,
        CancellationToken cancellationToken)
    {
        var subjectId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(subjectId))
        {
            return Results.Unauthorized();
        }

        var result = await handler.Handle(new GetProfileTopicsQuery(subjectId), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> SetTopicManualStatus(
        string topicId,
        SetTopicManualStatusRequest request,
        ICommandHandler<SetTopicManualStatusCommand, ProfileTopicDto> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new SetTopicManualStatusCommand(topicId, request.IsManual);
            var result = await handler.Handle(command, cancellationToken);
            return Results.Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }
}

public sealed record SetTopicManualStatusRequest(bool IsManual);
