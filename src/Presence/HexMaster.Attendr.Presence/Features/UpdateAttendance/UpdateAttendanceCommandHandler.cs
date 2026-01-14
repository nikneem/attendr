using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.IntegrationEvents.Events.Profiles;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Presence.Features.UpdateAttendance;

public sealed class UpdateAttendanceCommandHandler : ICommandHandler<UpdateAttendanceCommand>
{
    private readonly IConferencePresenceRepository _repository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ILogger<UpdateAttendanceCommandHandler> _logger;

    public UpdateAttendanceCommandHandler(
        IConferencePresenceRepository repository,
        IIntegrationEventPublisher eventPublisher,
        ILogger<UpdateAttendanceCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(UpdateAttendanceCommand command, CancellationToken cancellationToken)
    {
        using var activity = ActivitySources.Presence.StartActivity("UpdateAttendance", ActivityKind.Internal);
        activity?.SetTag("conference.id", command.ConferenceId);
        activity?.SetTag("profile.id", command.ProfileId);
        activity?.SetTag("is_attending", command.IsAttending);

        try
        {
            var presence = await _repository.GetAsync(command.ConferenceId, command.ProfileId, cancellationToken);

            if (presence is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Conference presence not found");
                _logger.LogWarning(
                    "Conference presence not found for ConferenceId: {ConferenceId}, ProfileId: {ProfileId}",
                    command.ConferenceId,
                    command.ProfileId);
                throw new InvalidOperationException("Conference presence not found.");
            }

            presence.UpdateAttendance(command.IsAttending);

            await _repository.UpdateAsync(presence, cancellationToken);

            var integrationEvent = new ProfileConferenceAttendanceChangedEvent
            {
                ProfileId = command.ProfileId,
                ConferenceId = command.ConferenceId,
                ConferenceName = presence.ConferenceName,
                IsAttending = command.IsAttending
            };

            await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(
                "Updated attendance for ConferenceId: {ConferenceId}, ProfileId: {ProfileId}, IsAttending: {IsAttending}",
                command.ConferenceId,
                command.ProfileId,
                command.IsAttending);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);

            _logger.LogError(ex,
                "Failed to update attendance for ConferenceId: {ConferenceId}, ProfileId: {ProfileId}",
                command.ConferenceId,
                command.ProfileId);
            throw;
        }
    }
}
