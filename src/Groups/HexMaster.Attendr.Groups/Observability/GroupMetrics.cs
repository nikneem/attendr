using System.Diagnostics.Metrics;

namespace HexMaster.Attendr.Groups.Observability;

/// <summary>
/// Metrics for group operations following OpenTelemetry semantic conventions.
/// </summary>
public sealed class GroupMetrics
{
    private readonly Counter<long> _groupsQueried;
    private readonly Counter<long> _groupsListed;
    private readonly Counter<long> _myGroupsQueried;
    private readonly Counter<long> _operationsFailed;
    private readonly Histogram<double> _operationDuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupMetrics"/> class.
    /// </summary>
    /// <param name="meterFactory">The meter factory for creating meters.</param>
    public GroupMetrics(IMeterFactory meterFactory)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);

        var meter = meterFactory.Create("HexMaster.Attendr.Groups", "1.0.0");

        _groupsQueried = meter.CreateCounter<long>(
            name: "groups.queried",
            unit: "{query}",
            description: "Total number of group detail queries");

        _groupsListed = meter.CreateCounter<long>(
            name: "groups.listed",
            unit: "{query}",
            description: "Total number of group list queries");

        _myGroupsQueried = meter.CreateCounter<long>(
            name: "groups.my_groups_queried",
            unit: "{query}",
            description: "Total number of my groups queries");

        _operationsFailed = meter.CreateCounter<long>(
            name: "groups.operations.failed",
            unit: "{operation}",
            description: "Total number of failed group operations");

        _operationDuration = meter.CreateHistogram<double>(
            name: "groups.operation.duration",
            unit: "ms",
            description: "Duration of group operations");
    }

    /// <summary>
    /// Records a group detail query.
    /// </summary>
    /// <param name="found">Whether the group was found.</param>
    public void RecordGroupQueried(bool found)
    {
        _groupsQueried.Add(1, new KeyValuePair<string, object?>("found", found));
    }

    /// <summary>
    /// Records a group list query.
    /// </summary>
    /// <param name="resultCount">Number of results returned.</param>
    public void RecordGroupsListed(int resultCount)
    {
        _groupsListed.Add(1, new KeyValuePair<string, object?>("result_count", resultCount));
    }

    /// <summary>
    /// Records a my groups query.
    /// </summary>
    /// <param name="resultCount">Number of groups the user is a member of.</param>
    public void RecordMyGroupsQueried(int resultCount)
    {
        _myGroupsQueried.Add(1, new KeyValuePair<string, object?>("result_count", resultCount));
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
