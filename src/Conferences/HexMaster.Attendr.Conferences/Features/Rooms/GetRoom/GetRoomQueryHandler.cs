using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.Rooms.GetRoom;

public sealed class GetRoomQueryHandler : IQueryHandler<GetRoomQuery, ConferenceRoomDto?>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<GetRoomQueryHandler> _logger;

    public GetRoomQueryHandler(IConferenceRepository repository, ILogger<GetRoomQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ConferenceRoomDto?> Handle(GetRoomQuery query, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Conferences.StartActivity("GetRoom", ActivityKind.Internal);
        activity?.SetTag("conference.id", query.ConferenceId);
        activity?.SetTag("room.id", query.RoomId);

        var conference = await _repository.GetByIdAsync(query.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference with ID {query.ConferenceId} not found");

        var room = conference.Rooms.FirstOrDefault(r => r.Id == query.RoomId);
        if (room == null) return null;
        return new ConferenceRoomDto(room.Id, room.Name, room.Capacity, room.ExternalId);
    }
}
