using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.IntegrationEvents.Events;
using HexMaster.Attendr.IntegrationEvents.Services;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Presence.Features.CheckIn;

/// <summary>
/// Command handler for checking in or out of a presentation.
/// </summary>
public sealed class CheckInCommandHandler : ICommandHandler<CheckInCommand>
{
    private readonly IConferencePresenceRepository _repository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    private readonly ILogger<CheckInCommandHandler> _logger;

    public CheckInCommandHandler(
        IConferencePresenceRepository repository,
        IIntegrationEventPublisher eventPublisher,
        ILogger<CheckInCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(CheckInCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        using var activity = ActivitySources.Presence.StartActivity("CheckIn", ActivityKind.Internal);
        activity?.SetTag("profile.id", command.ProfileId);
        activity?.SetTag("conference.id", command.ConferenceId);
        activity?.SetTag("presentation.id", command.PresentationId);
        activity?.SetTag("is_checked_in", command.IsCheckedIn);

        try
        {
            var conferencePresence = await _repository.GetAsync(command.ConferenceId, command.ProfileId, cancellationToken);

            if (conferencePresence is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Conference presence not found");
                _logger.LogWarning("Conference presence not found for profile {ProfileId} and conference {ConferenceId}",
                    command.ProfileId, command.ConferenceId);
                throw new KeyNotFoundException($"Conference presence not found for profile {command.ProfileId} and conference {command.ConferenceId}");
            }

            var presentation = conferencePresence.Presentations.FirstOrDefault(p => p.PresentationId == command.PresentationId);

            if (presentation is null)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Presentation not found");
                _logger.LogWarning("Presentation {PresentationId} not found in conference presence for profile {ProfileId}",
                    command.PresentationId, command.ProfileId);
                throw new KeyNotFoundException($"Presentation {command.PresentationId} not found in conference presence");
            }

            if (command.IsCheckedIn)
            {
                presentation.CheckIn();
                _logger.LogInformation("Profile {ProfileId} checked in to presentation {PresentationId}",
                    command.ProfileId, command.PresentationId);
            }
            else
            {
                presentation.CheckOut();
                _logger.LogInformation("Profile {ProfileId} checked out of presentation {PresentationId}",
                    command.ProfileId, command.PresentationId);
            }

            await _repository.UpdateAsync(conferencePresence, cancellationToken);

            var integrationEvent = new ProfileCheckedInEvent
            {
                ConferenceId = command.ConferenceId,
                PresentationId = command.PresentationId,
                Title = presentation.Title,
                StartDateTime = presentation.StartDateTime,
                EndDateTime = presentation.EndDateTime,
                Room = presentation.Room,
                ProfileId = command.ProfileId,
                IsCheckedIn = command.IsCheckedIn
            };

            await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation("Published ProfileCheckedInEvent for profile {ProfileId} and presentation {PresentationId}",
                command.ProfileId, command.PresentationId);
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);

            _logger.LogError(ex, "Failed to process check-in for profile {ProfileId} and presentation {PresentationId}",
                command.ProfileId, command.PresentationId);
            throw;
        }
    }
}
