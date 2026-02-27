using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.Rooms.ListRooms;

public sealed class ListRoomsQueryHandler : IQueryHandler<ListRoomsQuery, IReadOnlyList<ConferenceRoomDto>>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<ListRoomsQueryHandler> _logger;

    public ListRoomsQueryHandler(IConferenceRepository repository, ILogger<ListRoomsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ConferenceRoomDto>> Handle(ListRoomsQuery query, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Conferences.StartActivity("ListRooms", ActivityKind.Internal);
        activity?.SetTag("conference.id", query.ConferenceId);

        var conference = await _repository.GetByIdAsync(query.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference with ID {query.ConferenceId} not found");

        return conference.Rooms
            .Select(r => new ConferenceRoomDto(r.Id, r.Name, r.Capacity, r.ExternalId))
            .ToList();
    }
}
