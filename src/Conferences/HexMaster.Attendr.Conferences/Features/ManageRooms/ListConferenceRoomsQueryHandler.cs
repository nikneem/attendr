using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;

using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.ManageRooms;

public sealed class ListConferenceRoomsQueryHandler : IQueryHandler<ListConferenceRoomsQuery, List<ConferenceRoomDto>>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<ListConferenceRoomsQueryHandler> _logger;

    public ListConferenceRoomsQueryHandler(IConferenceRepository repository, ILogger<ListConferenceRoomsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<ConferenceRoomDto>> Handle(ListConferenceRoomsQuery query, CancellationToken cancellationToken = default)
    {
        var conference = await _repository.GetByIdAsync(query.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference {query.ConferenceId} not found.");

        _logger.LogInformation("Listing rooms for conference {ConferenceId}", query.ConferenceId);
        return conference.Rooms.Select(r => new ConferenceRoomDto(r.Id, r.Name, r.Capacity)).ToList();
    }
}
