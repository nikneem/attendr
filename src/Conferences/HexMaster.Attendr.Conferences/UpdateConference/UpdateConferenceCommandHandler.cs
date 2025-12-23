using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Observability;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.IntegrationEvents.Events;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Conferences.UpdateConference;

public sealed class UpdateConferenceCommandHandler : ICommandHandler<UpdateConferenceCommand, ConferenceDetailsDto>
{
    private readonly IConferenceRepository _repository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ConferenceMetrics _metrics;
    private readonly ILogger<UpdateConferenceCommandHandler> _logger;

    public UpdateConferenceCommandHandler(
        IConferenceRepository repository,
        IIntegrationEventPublisher eventPublisher,
        ConferenceMetrics metrics,
        ILogger<UpdateConferenceCommandHandler> logger)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<ConferenceDetailsDto> Handle(UpdateConferenceCommand command, CancellationToken cancellationToken)
    {
        using var activity = ActivitySources.Conferences.StartActivity("UpdateConference", ActivityKind.Internal);
        activity?.SetTag("conference.id", command.Id);
        activity?.SetTag("conference.title", command.Title);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Updating conference {ConferenceId}", command.Id);

            var conference = await _repository.GetByIdAsync(command.Id, cancellationToken);
            if (conference == null)
            {
                _logger.LogWarning("Conference {ConferenceId} not found for update", command.Id);
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

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordConferenceUpdated();
            _metrics.RecordOperationDuration("UpdateConference", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Conference {ConferenceId} updated successfully", conference.Id);

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
                        conference.SynchronizationSource.SourceLocationOrApiKey ?? string.Empty)
                    : null,
                new List<SpeakerDto>(),
                new List<PresentationDto>());
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("UpdateConference", ex.GetType().Name);
            _metrics.RecordOperationDuration("UpdateConference", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to update conference {ConferenceId}", command.Id);
            throw;
        }
    }
}
