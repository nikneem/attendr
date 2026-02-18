using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.IntegrationEvents.Events.Conferences;
using HexMaster.Attendr.IntegrationEvents.Models;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;

namespace HexMaster.Attendr.Conferences.Services;

/// <summary>
/// Service for analyzing presentation topics using AI.
/// </summary>
public sealed class PresentationTopicsAnalysisService
{
    private readonly ITopicsRepository _topicsRepository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly Kernel _kernel;
    private readonly ILogger<PresentationTopicsAnalysisService> _logger;

    public PresentationTopicsAnalysisService(
        ITopicsRepository topicsRepository,
        IIntegrationEventPublisher eventPublisher,
        Kernel kernel,
        ILogger<PresentationTopicsAnalysisService> logger)
    {
        _topicsRepository = topicsRepository ?? throw new ArgumentNullException(nameof(topicsRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Analyzes a presentation and extracts topics from its abstract.
    /// </summary>
    /// <param name="conferenceId">The conference ID.</param>
    /// <param name="presentation">The presentation to analyze.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task AnalyzeAsync(Guid conferenceId, Presentation presentation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(presentation);

        _logger.LogInformation("Starting topic analysis for presentation {PresentationId}", presentation.Id);

        try
        {
            // Extract topics using Semantic Kernel
            var topicKeys = await ExtractTopicsAsync(presentation.Abstract, cancellationToken);

            if (topicKeys.Count == 0)
            {
                _logger.LogWarning("No topics extracted for presentation {PresentationId}", presentation.Id);
            }

            // Match or create topics and link them to the presentation
            foreach (var topicKey in topicKeys)
            {
                var normalizedKey = NormalizeTopicKey(topicKey);
                var topic = await _topicsRepository.GetOrCreateTopicAsync(topicKey, cancellationToken);
                if (topic.IsVisible)
                {
                    presentation.AddTopic(new PresentationTopic(topic.Key, topic.Name));
                }
                await _topicsRepository.LinkPresentationToTopicAsync(presentation.Id, topic.Id, cancellationToken);
            }

            // Mark presentation as analyzed
            await _topicsRepository.MarkPresentationAsAnalysedAsync(presentation.Id, cancellationToken);

            var presentationUpdatedEvent = new PresentationUpdatedEvent
            {
                ConferenceId = conferenceId,
                PresentationId = presentation.Id,
                Title = presentation.Title,
                Abstract = presentation.Abstract,
                StartDateTime = presentation.StartDateTime,
                EndDateTime = presentation.EndDateTime,
                RoomId = presentation.Room.Id,
                RoomName = presentation.Room.Name,
                SpeakerIds = presentation.Speakers.Select(s => s.Id).ToList(),
                Topics = presentation.Topics.Select(t => new PresentationTopicDto(t.Key, t.Name)).ToList(),
                ExternalId = presentation.ExternalId,
                IsScheduleChanged = false
            };
            await _eventPublisher.PublishAsync(presentationUpdatedEvent, cancellationToken);

            _logger.LogInformation("Successfully analyzed presentation {PresentationId} ({PresentationTitle}) with {TopicCount} topics ({Topics})",
                presentation.Id, presentation.Title, topicKeys.Count, string.Join(", ", topicKeys));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze presentation {PresentationId}", presentation.Id);
            throw;
        }
    }

    private async Task<List<string>> ExtractTopicsAsync(string abstractText, CancellationToken cancellationToken)
    {
        var prompt = @$"You are an expert at analyzing technical conference presentation abstracts.

IMPORTANT: First, call the get_existing_topics function to see what topics already exist in the system.
When existing topics are semantically similar to the content you're analyzing, STRONGLY PREFER to reuse those existing topic names.
Only create NEW topics if they represent distinctly different concepts not adequately covered by existing topics.

Your task is to extract the main topics and themes from the presentation abstract.
Return ONLY a valid JSON object with a 'topics' property containing an array of topic strings.

Topics should be:
- Concise (1-3 words)
- Relevant technical concepts, technologies, or methodologies
- Maximum 5 topics per presentation
- Formatted in title case
- PREFER existing topics when semantically similar to avoid topic fragmentation

Example response format:
{{""topics"": [""Azure Functions"", ""Serverless"", ""Cloud Architecture""]}}

Presentation abstract to analyze:
{abstractText}";

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ResponseFormat = typeof(ConferencePresentationTopics),
        };

        var response = await _kernel.InvokePromptAsync(
            prompt,
            new KernelArguments(executionSettings),
            cancellationToken: cancellationToken);

        var conferenceTopics =
            JsonConvert.DeserializeObject<ConferencePresentationTopics>(response.ToString());

        return conferenceTopics?.Topics ?? new List<string>();
    }

    private static string NormalizeTopicKey(string topic)
    {
        // Convert to lowercase and replace spaces with hyphens for consistent keys
        return topic.ToLowerInvariant().Replace(" ", "-");
    }
}

/// <summary>
/// Response model for AI-generated presentation topics.
/// </summary>
public record ConferencePresentationTopics(List<string> Topics);
