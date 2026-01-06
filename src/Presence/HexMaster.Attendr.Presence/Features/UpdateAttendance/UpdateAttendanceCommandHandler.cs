using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Presence;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Presence.Features.UpdateAttendance;

public sealed class UpdateAttendanceCommandHandler : ICommandHandler<UpdateAttendanceCommand>
{
    private readonly IConferencePresenceRepository _repository;
    private readonly ILogger<UpdateAttendanceCommandHandler> _logger;

    public UpdateAttendanceCommandHandler(
        IConferencePresenceRepository repository,
        ILogger<UpdateAttendanceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(UpdateAttendanceCommand command, CancellationToken cancellationToken)
    {
        var presence = await _repository.GetAsync(command.ConferenceId, command.ProfileId, cancellationToken);

        if (presence is null)
        {
            _logger.LogWarning(
                "Conference presence not found for ConferenceId: {ConferenceId}, ProfileId: {ProfileId}",
                command.ConferenceId,
                command.ProfileId);
            throw new InvalidOperationException("Conference presence not found.");
        }

        presence.UpdateAttendance(command.IsAttending);

        await _repository.UpdateAsync(presence, cancellationToken);

        _logger.LogInformation(
            "Updated attendance for ConferenceId: {ConferenceId}, ProfileId: {ProfileId}, IsAttending: {IsAttending}",
            command.ConferenceId,
            command.ProfileId,
            command.IsAttending);
    }
}
