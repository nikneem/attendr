using System.Diagnostics;
using HexMaster.Attendr.Conferences.Observability;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.DeleteConference;

public sealed class DeleteConferenceCommandHandler : ICommandHandler<DeleteConferenceCommand, bool>
{
    private readonly IConferenceRepository _repository;
    private readonly ConferenceMetrics _metrics;
    private readonly ILogger<DeleteConferenceCommandHandler> _logger;

    public DeleteConferenceCommandHandler(
        IConferenceRepository repository,
        ConferenceMetrics metrics,
        ILogger<DeleteConferenceCommandHandler> logger)
    {
        _repository = repository;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteConferenceCommand command, CancellationToken cancellationToken)
    {
        using var activity = ActivitySources.Conferences.StartActivity("DeleteConference", ActivityKind.Internal);
        activity?.SetTag("conference.id", command.Id);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Deleting conference {ConferenceId}", command.Id);

            var deleted = await _repository.DeleteAsync(command.Id, cancellationToken);

            if (!deleted)
            {
                _logger.LogWarning("Conference {ConferenceId} not found for deletion", command.Id);
                activity?.SetStatus(ActivityStatusCode.Error, "Conference not found");
                _metrics.RecordOperationFailed("DeleteConference", "NotFound");
                _metrics.RecordOperationDuration("DeleteConference", stopwatch.Elapsed.TotalMilliseconds, success: false);
                return false;
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("DeleteConference", stopwatch.Elapsed.TotalMilliseconds, success: true);

            _logger.LogInformation("Conference {ConferenceId} deleted successfully", command.Id);
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("DeleteConference", ex.GetType().Name);
            _metrics.RecordOperationDuration("DeleteConference", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to delete conference {ConferenceId}", command.Id);
            throw;
        }
    }
}
