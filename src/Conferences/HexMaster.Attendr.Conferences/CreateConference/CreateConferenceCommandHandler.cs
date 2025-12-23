using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Abstractions.Services;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Observability;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.IntegrationEvents.Events;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Conferences.CreateConference;

/// <summary>
/// Command handler to create a new conference.
/// Implements distributed tracing via OpenTelemetry and structured logging.
/// </summary>
public sealed class CreateConferenceCommandHandler : ICommandHandler<CreateConferenceCommand, CreateConferenceResult>
{
    private readonly IConferenceRepository _conferenceRepository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ConferenceMetrics _metrics;
    private readonly ILogger<CreateConferenceCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateConferenceCommandHandler"/> class.
    /// </summary>
    /// <param name="conferenceRepository">The conference repository.</param>
    /// <param name="eventPublisher">The integration event publisher.</param>
    /// <param name="metrics">The metrics for recording conference operations.</param>
    /// <param name="logger">Logger for recording operation details and errors.</param>
    public CreateConferenceCommandHandler(
        IConferenceRepository conferenceRepository,
        IIntegrationEventPublisher eventPublisher,
        ConferenceMetrics metrics,
        ILogger<CreateConferenceCommandHandler> logger)
    {
        _conferenceRepository = conferenceRepository ?? throw new ArgumentNullException(nameof(conferenceRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the CreateConferenceCommand.
    /// </summary>
    /// <param name="command">The command containing conference details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created conference result.</returns>
    public async Task<CreateConferenceResult> Handle(
        CreateConferenceCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Conferences.StartActivity("CreateConference", ActivityKind.Internal);
        activity?.SetTag("conference.title", command.Title);
        activity?.SetTag("conference.city", command.City);
        activity?.SetTag("conference.country", command.Country);
        activity?.SetTag("conference.has_sync_source", command.SynchronizationSource is not null);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Creating conference: {Title} in {City}, {Country}",
                command.Title, command.City, command.Country);

            // Create synchronization source if provided
            SynchronizationSource? syncSource = null;
            if (command.SynchronizationSource is not null)
            {
                if (!Enum.TryParse<SynchronizationSourceType>(command.SynchronizationSource.SourceType, true, out var sourceType))
                {
                    _logger.LogWarning("Invalid synchronization source type: {SourceType}",
                        command.SynchronizationSource.SourceType);
                    throw new ArgumentException($"Invalid synchronization source type: {command.SynchronizationSource.SourceType}");
                }

                syncSource = SynchronizationSource.CreateWithUrl(sourceType, command.SynchronizationSource.SourceUrl);
                activity?.SetTag("conference.sync_source_type", sourceType.ToString());
            }

            // Create the conference
            var conference = Conference.Create(
                command.Title,
                command.City,
                command.Country,
                command.StartDate,
                command.EndDate,
                command.ImageUrl,
                syncSource);

            activity?.SetTag("conference.id", conference.Id);

            // Persist the conference
            await _conferenceRepository.AddAsync(conference, cancellationToken);

            // Publish integration event
            var conferenceCreatedEvent = new ConferenceCreatedEvent
            {
                ConferenceId = conference.Id,
                Title = conference.Title,
                City = conference.City,
                Country = conference.Country,
                StartDate = conference.StartDate,
                EndDate = conference.EndDate
            };
            await _eventPublisher.PublishAsync(conferenceCreatedEvent, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordConferenceCreated(syncSource is not null);
            _metrics.RecordOperationDuration("CreateConference", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Conference {ConferenceId} created successfully: {Title}",
                conference.Id, conference.Title);

            // Return result
            return new CreateConferenceResult(
                conference.Id,
                conference.Title,
                conference.City,
                conference.Country,
                conference.StartDate,
                conference.EndDate);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("CreateConference", ex.GetType().Name);
            _metrics.RecordOperationDuration("CreateConference", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to create conference: {Title}", command.Title);
            throw;
        }
    }
}
