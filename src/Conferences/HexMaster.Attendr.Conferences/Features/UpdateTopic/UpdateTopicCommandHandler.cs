using System.Diagnostics;
using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Observability;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.IntegrationEvents.Events.Conferences;
using HexMaster.Attendr.IntegrationEvents.Events.Topics;
using HexMaster.Attendr.IntegrationEvents.Models;
using HexMaster.Attendr.IntegrationEvents.Services;
using IntegrationSpeakerDto = HexMaster.Attendr.IntegrationEvents.Models.SpeakerDto;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.UpdateTopic;

/// <summary>
/// Command handler to update an existing topic.
/// </summary>
public sealed class UpdateTopicCommandHandler : ICommandHandler<UpdateTopicCommand, TopicDto>
{
    private readonly ITopicsRepository _topicsRepository;
    private readonly IConferenceRepository _conferenceRepository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ConferenceMetrics _metrics;
    private readonly ILogger<UpdateTopicCommandHandler> _logger;

    public UpdateTopicCommandHandler(
        ITopicsRepository topicsRepository,
        IConferenceRepository conferenceRepository,
        IIntegrationEventPublisher eventPublisher,
        ConferenceMetrics metrics,
        ILogger<UpdateTopicCommandHandler> logger)
    {
        _topicsRepository = topicsRepository ?? throw new ArgumentNullException(nameof(topicsRepository));
        _conferenceRepository = conferenceRepository ?? throw new ArgumentNullException(nameof(conferenceRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TopicDto> Handle(UpdateTopicCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Conferences.StartActivity("UpdateTopic", ActivityKind.Internal);
        activity?.SetTag("topic.id", command.Id);
        activity?.SetTag("topic.key", command.Key);
        activity?.SetTag("topic.is_visible", command.IsVisible);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Updating topic {TopicId}", command.Id);

            var topic = await _topicsRepository.GetTopicByIdAsync(command.Id, cancellationToken);
            if (topic == null)
            {
                _logger.LogWarning("Topic {TopicId} not found for update", command.Id);
                throw new KeyNotFoundException($"Topic with ID {command.Id} not found");
            }

            // Update topic properties
            // Key and Name are updated together to maintain consistency
            topic.UpdateDetails(command.Key, command.Name);

            // Update visibility
            if (command.IsVisible && !topic.IsVisible)
            {
                topic.MakeVisible();
            }
            else if (!command.IsVisible && topic.IsVisible)
            {
                topic.Hide();
            }

            await _topicsRepository.UpdateTopicAsync(topic, cancellationToken);

            var topicChangedEvent = new TopicChangedEvent
            {
                TopicId = topic.Id,
                Key = topic.Key,
                Name = topic.Name,
                IsVisible = topic.IsVisible
            };
            await _eventPublisher.PublishAsync(topicChangedEvent, cancellationToken);

            _logger.LogInformation("Published TopicChangedEvent for topic {TopicId}", topic.Id);

            // Find all future presentations with this topic and publish PresentationUpdatedEvent
            var affectedPresentations = await _topicsRepository.GetFuturePresentationsByTopicIdAsync(command.Id, cancellationToken);

            _logger.LogInformation("Found {Count} future presentations affected by topic update", affectedPresentations.Count);

            foreach (var (conferenceId, presentationId) in affectedPresentations)
            {
                // Load presentation with speakers and topics using the new method
                var presentation = await _conferenceRepository.GetPresentationByIdAsync(conferenceId, presentationId, cancellationToken);
                if (presentation == null)
                {
                    _logger.LogWarning("Presentation {PresentationId} not found in conference {ConferenceId}", presentationId, conferenceId);
                    continue;
                }

                // Publish PresentationUpdatedEvent with updated topics
                var integrationEvent = new PresentationUpdatedEvent
                {
                    ConferenceId = conferenceId,
                    PresentationId = presentationId,
                    Title = presentation.Title,
                    Abstract = presentation.Abstract,
                    StartDateTime = presentation.StartDateTime,
                    EndDateTime = presentation.EndDateTime,
                    RoomId = presentation.Room.Id,
                    RoomName = presentation.Room.Name,
                    Speakers = presentation.Speakers.Select(s => new IntegrationSpeakerDto(s.Id, s.Name, s.ProfilePictureUrl)).ToList(),
                    Topics = presentation.Topics.Select(t => new PresentationTopicDto(t.Key, t.Name)).ToList(),
                    ExternalId = presentation.ExternalId,
                    IsScheduleChanged = false  // Topic update doesn't change schedule
                };

                await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);

                _logger.LogInformation(
                    "Published PresentationUpdatedEvent for presentation {PresentationId} due to topic {TopicId} update",
                    presentationId,
                    command.Id);
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("UpdateTopic", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Topic {TopicId} updated successfully", topic.Id);

            return new TopicDto(topic.Id, topic.Key, topic.Name, topic.IsVisible);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("UpdateTopic", ex.GetType().Name);
            _metrics.RecordOperationDuration("UpdateTopic", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to update topic {TopicId}", command.Id);
            throw;
        }
    }
}
