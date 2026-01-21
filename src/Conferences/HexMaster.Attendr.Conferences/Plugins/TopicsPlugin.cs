using System.ComponentModel;
using HexMaster.Attendr.Conferences.DomainModels;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace HexMaster.Attendr.Conferences.Plugins;

/// <summary>
/// Semantic Kernel plugin that provides access to existing topics.
/// This plugin allows the AI to query existing topics and encourage reuse of similar topics
/// while allowing creation of new topics when they are sufficiently different.
/// </summary>
public sealed class TopicsPlugin
{
    private readonly ITopicsRepository _topicsRepository;
    private readonly ILogger<TopicsPlugin> _logger;

    public TopicsPlugin(
        ITopicsRepository topicsRepository,
        ILogger<TopicsPlugin> logger)
    {
        _topicsRepository = topicsRepository ?? throw new ArgumentNullException(nameof(topicsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all existing visible topics from the repository.
    /// Use this function to see what topics already exist before suggesting new topics.
    /// When analyzing presentation topics, prefer to match existing topics if they are semantically similar.
    /// Only suggest new topics if they represent distinctly different concepts.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of existing topic names that can be reused.</returns>
    [KernelFunction("get_existing_topics")]
    [Description("Retrieves all existing visible topics. Use this to encourage reuse of existing topics when analyzing presentations. Only create new topics if they are sufficiently different from existing ones.")]
    public async Task<string> GetExistingTopicsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Plugin: Fetching existing topics for AI analysis");

            var topics = await _topicsRepository.ListTopicsAsync(onlyVisible: false, cancellationToken);

            if (topics.Count == 0)
            {
                _logger.LogInformation("Plugin: No existing topics found");
                return "No existing topics available. You can create new topics freely.";
            }

            var topicNames = topics
                .OrderBy(t => t.Name)
                .Select(t => t.Name)
                .ToList();

            var result = $"Existing topics ({topicNames.Count}): {string.Join(", ", topicNames)}. " +
                         "When analyzing presentations, strongly prefer to reuse these topics if semantically similar. " +
                         "Only create new topics if they represent distinctly different concepts not covered by existing topics.";

            _logger.LogInformation("Plugin: Returned {TopicCount} existing topics to AI", topicNames.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Plugin: Failed to fetch existing topics");
            return "Unable to fetch existing topics. You may proceed with topic creation as needed.";
        }
    }
}
