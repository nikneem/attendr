using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.GetConference;

/// <summary>
/// Query handler for retrieving a specific conference by ID.
/// </summary>
public sealed class GetConferenceQueryHandler : IQueryHandler<GetConferenceQuery, ConferenceDetailsDto?>
{
    private readonly IConferenceRepository _conferenceRepository;

    public GetConferenceQueryHandler(IConferenceRepository conferenceRepository)
    {
        _conferenceRepository = conferenceRepository;
    }

    public async Task<ConferenceDetailsDto?> Handle(GetConferenceQuery query, CancellationToken cancellationToken)
    {
        var conference = await _conferenceRepository.GetByIdAsync(query.ConferenceId, cancellationToken);

        if (conference == null)
        {
            return null;
        }

        SynchronizationSourceDto? syncSourceDto = null;
        if (conference.SynchronizationSource != null)
        {
            syncSourceDto = new SynchronizationSourceDto(
                conference.SynchronizationSource.SourceType.ToString(),
                conference.SynchronizationSource.SourceUrl ?? string.Empty);
        }

        return new ConferenceDetailsDto(
            conference.Id,
            conference.Title,
            conference.City,
            conference.Country,
            conference.StartDate,
            conference.EndDate,
            conference.ImageUrl,
            syncSourceDto);
    }
}
