using System.Diagnostics.Metrics;

namespace HexMaster.Attendr.Profiles.Observability;

/// <summary>
/// Metrics for profile operations following OpenTelemetry semantic conventions.
/// </summary>
public sealed class ProfileMetrics
{
    private readonly Counter<long> _profilesCreated;
    private readonly Counter<long> _profilesExisting;
    private readonly Counter<long> _operationsFailed;
    private readonly Histogram<double> _operationDuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileMetrics"/> class.
    /// </summary>
    /// <param name="meterFactory">The meter factory for creating meters.</param>
    public ProfileMetrics(IMeterFactory meterFactory)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);

        var meter = meterFactory.Create("HexMaster.Attendr.Profiles", "1.0.0");

        _profilesCreated = meter.CreateCounter<long>(
            name: "profiles.created",
            unit: "{profile}",
            description: "Total number of profiles successfully created");

        _profilesExisting = meter.CreateCounter<long>(
            name: "profiles.existing",
            unit: "{profile}",
            description: "Total number of create requests for existing profiles");

        _operationsFailed = meter.CreateCounter<long>(
            name: "profiles.operations.failed",
            unit: "{operation}",
            description: "Total number of failed profile operations");

        _operationDuration = meter.CreateHistogram<double>(
            name: "profiles.operation.duration",
            unit: "ms",
            description: "Duration of profile operations");
    }

    /// <summary>
    /// Records a profile creation.
    /// </summary>
    public void RecordProfileCreated()
    {
        _profilesCreated.Add(1);
    }

    /// <summary>
    /// Records a request for an existing profile.
    /// </summary>
    public void RecordProfileExisting()
    {
        _profilesExisting.Add(1);
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
