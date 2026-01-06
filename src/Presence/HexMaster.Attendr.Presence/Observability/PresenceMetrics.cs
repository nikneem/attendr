using System.Diagnostics.Metrics;

namespace HexMaster.Attendr.Presence.Observability;

/// <summary>
/// Metrics for presence operations following OpenTelemetry semantic conventions.
/// </summary>
public sealed class PresenceMetrics
{
    private readonly Counter<long> _presencesCreated;
    private readonly Counter<long> _presentationsRated;
    private readonly Counter<long> _presentationsUpdated;
    private readonly Counter<long> _conferencesQueried;
    private readonly Counter<long> _presentationsQueried;
    private readonly Counter<long> _operationsFailed;
    private readonly Histogram<double> _operationDuration;
    private readonly Histogram<int> _ratingValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="PresenceMetrics"/> class.
    /// </summary>
    /// <param name="meterFactory">The meter factory for creating meters.</param>
    public PresenceMetrics(IMeterFactory meterFactory)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);

        var meter = meterFactory.Create("HexMaster.Attendr.Presence", "1.0.0");

        _presencesCreated = meter.CreateCounter<long>(
            name: "presence.created",
            unit: "{presence}",
            description: "Total number of conference presences successfully created");

        _presentationsRated = meter.CreateCounter<long>(
            name: "presence.presentations.rated",
            unit: "{rating}",
            description: "Total number of presentation ratings submitted");

        _presentationsUpdated = meter.CreateCounter<long>(
            name: "presence.presentations.updated",
            unit: "{presentation}",
            description: "Total number of presentation presences updated");

        _conferencesQueried = meter.CreateCounter<long>(
            name: "presence.conferences.queried",
            unit: "{query}",
            description: "Total number of conference presence queries");

        _presentationsQueried = meter.CreateCounter<long>(
            name: "presence.presentations.queried",
            unit: "{query}",
            description: "Total number of presentation queries for rating");

        _operationsFailed = meter.CreateCounter<long>(
            name: "presence.operations.failed",
            unit: "{operation}",
            description: "Total number of failed presence operations");

        _operationDuration = meter.CreateHistogram<double>(
            name: "presence.operation.duration",
            unit: "ms",
            description: "Duration of presence operations");

        _ratingValue = meter.CreateHistogram<int>(
            name: "presence.rating.value",
            unit: "{rating}",
            description: "Distribution of rating values (0-5)");
    }

    /// <summary>
    /// Records a conference presence creation.
    /// </summary>
    /// <param name="profileCount">Number of profiles for which presence was created.</param>
    public void RecordPresenceCreated(int profileCount)
    {
        _presencesCreated.Add(profileCount, new KeyValuePair<string, object?>("profile_count", profileCount));
    }

    /// <summary>
    /// Records a presentation rating.
    /// </summary>
    /// <param name="rating">The rating value (0-5).</param>
    /// <param name="isFavorite">Whether the presentation was marked as favorite.</param>
    public void RecordPresentationRated(int rating, bool isFavorite)
    {
        _presentationsRated.Add(1, new KeyValuePair<string, object?>("is_favorite", isFavorite));
        _ratingValue.Record(rating, new KeyValuePair<string, object?>("is_favorite", isFavorite));
    }

    /// <summary>
    /// Records a presentation update.
    /// </summary>
    /// <param name="affectedCount">Number of presentation presences updated.</param>
    /// <param name="scheduleChanged">Whether the schedule was changed.</param>
    public void RecordPresentationUpdated(int affectedCount, bool scheduleChanged)
    {
        _presentationsUpdated.Add(affectedCount,
            new KeyValuePair<string, object?>("affected_count", affectedCount),
            new KeyValuePair<string, object?>("schedule_changed", scheduleChanged));
    }

    /// <summary>
    /// Records a conference query.
    /// </summary>
    /// <param name="resultCount">Number of conferences returned.</param>
    public void RecordConferencesQueried(int resultCount)
    {
        _conferencesQueried.Add(1, new KeyValuePair<string, object?>("result_count", resultCount));
    }

    /// <summary>
    /// Records a presentation query for rating.
    /// </summary>
    /// <param name="found">Whether an unrated presentation was found.</param>
    /// <param name="unratedCount">Total number of unrated presentations available.</param>
    public void RecordPresentationQueried(bool found, int unratedCount)
    {
        _presentationsQueried.Add(1,
            new KeyValuePair<string, object?>("found", found),
            new KeyValuePair<string, object?>("unrated_count", unratedCount));
    }

    /// <summary>
    /// Records a failed operation.
    /// </summary>
    /// <param name="operationType">The type of operation that failed.</param>
    /// <param name="errorType">The type of error that occurred.</param>
    public void RecordOperationFailed(string operationType, string errorType)
    {
        _operationsFailed.Add(1,
            new KeyValuePair<string, object?>("operation_type", operationType),
            new KeyValuePair<string, object?>("error_type", errorType));
    }

    /// <summary>
    /// Records the duration of an operation.
    /// </summary>
    /// <param name="operationType">The type of operation.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    /// <param name="success">Whether the operation succeeded.</param>
    public void RecordOperationDuration(string operationType, double durationMs, bool success)
    {
        _operationDuration.Record(durationMs,
            new KeyValuePair<string, object?>("operation_type", operationType),
            new KeyValuePair<string, object?>("success", success));
    }
}
