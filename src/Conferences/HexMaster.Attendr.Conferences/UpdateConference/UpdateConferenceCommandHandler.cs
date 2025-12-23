using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.IntegrationEvents.Events;
using HexMaster.Attendr.IntegrationEvents.Services;

namespace HexMaster.Attendr.Conferences.UpdateConference;

public sealed class UpdateConferenceCommandHandler : ICommandHandler<UpdateConferenceCommand, ConferenceDetailsDto>
{
    private readonly IConferenceRepository _repository;
    private readonly IIntegrationEventPublisher _eventPublisher;

    public UpdateConferenceCommandHandler(
        IConferenceRepository repository,
        IIntegrationEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
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

        // Publish integration event
        var conferenceUpdatedEvent = new ConferenceUpdatedEvent
        {
            ConferenceId = conference.Id,
            Title = conference.Title,
            City = conference.City,
            Country = conference.Country,
            StartDate = conference.StartDate,
            EndDate = conference.EndDate
        };
        await _eventPublisher.PublishAsync(conferenceUpdatedEvent, cancellationToken);

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
