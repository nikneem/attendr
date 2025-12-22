using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.UpdateConference;

public sealed class UpdateConferenceCommandHandler : ICommandHandler<UpdateConferenceCommand, ConferenceDetailsDto>
{
    private readonly IConferenceRepository _repository;

    public UpdateConferenceCommandHandler(IConferenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<ConferenceDetailsDto> Handle(UpdateConferenceCommand command, CancellationToken cancellationToken)
    {
        var conference = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (conference == null)
        {
            throw new KeyNotFoundException($"Conference with ID {command.Id} not found");
        }

        conference.UpdateDetails(
            command.Title,
            command.City,
            command.Country,
            command.StartDate,
            command.EndDate,
            command.ImageUrl);

        if (command.SynchronizationSource != null)
        {
            var sourceType = Enum.Parse<SynchronizationSourceType>(command.SynchronizationSource.SourceType);
            var syncSource = SynchronizationSource.CreateWithUrl(sourceType, command.SynchronizationSource.SourceUrl);
            conference.ConfigureSynchronizationSource(syncSource);
        }
        else
        {
            conference.ConfigureSynchronizationSource(null);
        }

        await _repository.UpdateAsync(conference, cancellationToken);

        return new ConferenceDetailsDto(
            conference.Id,
            conference.Title,
            conference.City,
            conference.Country,
            conference.StartDate,
            conference.EndDate,
            conference.ImageUrl,
            conference.SynchronizationSource != null
                ? new SynchronizationSourceDto(
                    conference.SynchronizationSource.SourceType.ToString(),
                    conference.SynchronizationSource.SourceUrl ?? string.Empty)
                : null);
    }
}
