using System.Diagnostics.Metrics;

namespace HexMaster.Attendr.Conferences.Observability;

/// <summary>
/// Metrics for conference operations following OpenTelemetry semantic conventions.
/// </summary>
public sealed class ConferenceMetrics
{
    private readonly Counter<long> _conferencesCreated;
    private readonly Counter<long> _conferencesUpdated;
    private readonly Counter<long> _conferencesQueried;
    private readonly Counter<long> _conferencesListed;
    private readonly Counter<long> _operationsFailed;
    private readonly Histogram<double> _operationDuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConferenceMetrics"/> class.
    /// </summary>
    /// <param name="meterFactory">The meter factory for creating meters.</param>
    public ConferenceMetrics(IMeterFactory meterFactory)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);

        var meter = meterFactory.Create("HexMaster.Attendr.Conferences", "1.0.0");

        _conferencesCreated = meter.CreateCounter<long>(
            name: "conferences.created",
            unit: "{conference}",
            description: "Total number of conferences successfully created");

        _conferencesUpdated = meter.CreateCounter<long>(
            name: "conferences.updated",
            unit: "{conference}",
            description: "Total number of conferences successfully updated");

        _conferencesQueried = meter.CreateCounter<long>(
            name: "conferences.queried",
            unit: "{query}",
            description: "Total number of conference detail queries");

        _conferencesListed = meter.CreateCounter<long>(
            name: "conferences.listed",
            unit: "{query}",
            description: "Total number of conference list queries");

        _operationsFailed = meter.CreateCounter<long>(
            name: "conferences.operations.failed",
            unit: "{operation}",
            description: "Total number of failed conference operations");

        _operationDuration = meter.CreateHistogram<double>(
            name: "conferences.operation.duration",
            unit: "ms",
            description: "Duration of conference operations");
    }

    /// <summary>
    /// Records a conference creation.
    /// </summary>
    /// <param name="hasSyncSource">Whether the conference has a synchronization source.</param>
    public void RecordConferenceCreated(bool hasSyncSource)
    {
        _conferencesCreated.Add(1, new KeyValuePair<string, object?>("has_sync_source", hasSyncSource));
    }

    /// <summary>
    /// Records a conference update.
    /// </summary>
    public void RecordConferenceUpdated()
    {
        _conferencesUpdated.Add(1);
    }

    /// <summary>
    /// Records a conference query.
    /// </summary>
    /// <param name="found">Whether the conference was found.</param>
    public void RecordConferenceQueried(bool found)
    {
        _conferencesQueried.Add(1, new KeyValuePair<string, object?>("found", found));
    }

    /// <summary>
    /// Records a conference list query.
    /// </summary>
    /// <param name="resultCount">Number of results returned.</param>
    public void RecordConferencesListed(int resultCount)
    {
        _conferencesListed.Add(1, new KeyValuePair<string, object?>("result_count", resultCount));
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
