using HexMaster.Attendr.Core.CommandHandlers;

using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.ManagePresentations;

public sealed class DeleteConferencePresentationCommandHandler : ICommandHandler<DeleteConferencePresentationCommand, bool>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<DeleteConferencePresentationCommandHandler> _logger;

    public DeleteConferencePresentationCommandHandler(IConferenceRepository repository, ILogger<DeleteConferencePresentationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteConferencePresentationCommand command, CancellationToken cancellationToken = default)
    {
        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference {command.ConferenceId} not found.");

        conference.RemovePresentation(command.PresentationId);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Deleted presentation {PresentationId} from conference {ConferenceId}", command.PresentationId, command.ConferenceId);
        return true;
    }
}
