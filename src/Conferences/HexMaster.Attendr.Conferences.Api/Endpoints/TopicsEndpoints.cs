using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Api.Authorization;
using HexMaster.Attendr.Conferences.Features.CreateTopic;
using HexMaster.Attendr.Conferences.Features.GetTopic;
using HexMaster.Attendr.Conferences.Features.ListTopics;
using HexMaster.Attendr.Conferences.Features.UpdateTopic;
using HexMaster.Attendr.Conferences.Features.DeleteTopic;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;

namespace HexMaster.Attendr.Conferences.Api.Endpoints;

/// <summary>
/// Endpoints for topic CRUD operations.
/// All operations require admin authorization.
/// </summary>
public static class TopicsEndpoints
{
    public static IEndpointRouteBuilder MapTopicsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/topics")
            .WithName("Topics")
            .RequireAuthorization(AuthorizationPolicies.Admin);

        group.MapGet("/", ListTopics)
            .WithName("ListTopics")
            .Produces<ListTopicsResult>(StatusCodes.Status200OK)
            .WithSummary("List all topics");

        group.MapGet("/{id:guid}", GetTopic)
            .WithName("GetTopic")
            .Produces<TopicDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get a specific topic by ID");

        group.MapPost("/", CreateTopic)
            .WithName("CreateTopic")
            .Produces<TopicDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Create a new topic");

        group.MapPut("/{id:guid}", UpdateTopic)
            .WithName("UpdateTopic")
            .Produces<TopicDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update an existing topic (ID cannot be changed)");

        group.MapDelete("/{id:guid}", DeleteTopic)
            .WithName("DeleteTopic")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete a topic (cascade deletes all presentation references)");

        return app;
    }

    private static async Task<IResult> ListTopics(
        IQueryHandler<ListTopicsQuery, ListTopicsResult> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ListTopicsQuery(OnlyVisible: false), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetTopic(
        Guid id,
        IQueryHandler<GetTopicQuery, TopicDto?> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetTopicQuery(id), cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> CreateTopic(
        CreateTopicRequest request,
        ICommandHandler<CreateTopicCommand, TopicDto> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateTopicCommand(request.Key, request.Name);
            var result = await handler.Handle(command, cancellationToken);
            return Results.Created($"/api/topics/{result.Id}", result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> UpdateTopic(
        Guid id,
        UpdateTopicRequest request,
        ICommandHandler<UpdateTopicCommand, TopicDto> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateTopicCommand(id, request.Key, request.Name, request.IsVisible);
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

    private static async Task<IResult> DeleteTopic(
        Guid id,
        ICommandHandler<DeleteTopicCommand, bool> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteTopicCommand(id);
            var result = await handler.Handle(command, cancellationToken);
            return result ? Results.NoContent() : Results.NotFound();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }
}
