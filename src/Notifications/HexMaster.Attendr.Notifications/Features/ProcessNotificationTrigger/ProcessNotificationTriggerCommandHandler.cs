using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Notifications.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Notifications.Features.ProcessNotificationTrigger;

/// <summary>
/// Command to process a notification trigger event.
/// </summary>
public sealed record ProcessNotificationTriggerCommand(
    Guid ProfileId,
    string TypeKey,
    string Title,
    string Message,
    string? Url = null,
    Guid? ActorId = null,
    Dictionary<string, string>? EntityRefs = null,
    string? StackKey = null) : ICommand;

/// <summary>
/// Handler for processing notification trigger commands.
/// </summary>
public sealed class ProcessNotificationTriggerCommandHandler : ICommandHandler<ProcessNotificationTriggerCommand>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<ProcessNotificationTriggerCommandHandler> _logger;

    public ProcessNotificationTriggerCommandHandler(
        INotificationService notificationService,
        ILogger<ProcessNotificationTriggerCommandHandler> logger)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(ProcessNotificationTriggerCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Processing notification trigger for profile {ProfileId} of type {TypeKey}",
                command.ProfileId, command.TypeKey);

            await _notificationService.CreateNotificationAsync(
                command.ProfileId,
                command.TypeKey,
                command.Title,
                command.Message,
                command.Url,
                command.ActorId,
                command.EntityRefs,
                command.StackKey,
                cancellationToken);

            _logger.LogInformation(
                "Successfully processed notification trigger for profile {ProfileId}",
                command.ProfileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process notification trigger for profile {ProfileId} of type {TypeKey}",
                command.ProfileId, command.TypeKey);
            throw;
        }
    }
}
