using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Exceptions;
using HexMaster.Attendr.Core.Observability;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Conferences.Features.Presentations.DeletePresentation;

public sealed class DeletePresentationCommandHandler : ICommandHandler<DeletePresentationCommand, bool>
{
    private readonly IConferenceRepository _repository;
    private readonly ILogger<DeletePresentationCommandHandler> _logger;

    public DeletePresentationCommandHandler(IConferenceRepository repository, ILogger<DeletePresentationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeletePresentationCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        using var activity = ActivitySources.Conferences.StartActivity("DeletePresentation", ActivityKind.Internal);
        activity?.SetTag("conference.id", command.ConferenceId);
        activity?.SetTag("presentation.id", command.PresentationId);

        var conference = await _repository.GetByIdAsync(command.ConferenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conference with ID {command.ConferenceId} not found");

        if (!command.IsAdmin && conference.CreatedByProfileId != command.RequestingProfileId)
            throw new ForbiddenException("You are not authorized to modify this conference.", "https://attendr.dev/errors/forbidden");

        if (!conference.Presentations.Any(p => p.Id == command.PresentationId))
            return false;

        conference.RemovePresentation(command.PresentationId);
        conference.MarkInvisibleDueToManualChanges();

        await _repository.UpdateAsync(conference, cancellationToken);

        _logger.LogInformation("Presentation {PresentationId} deleted from conference {ConferenceId}", command.PresentationId, command.ConferenceId);

        return true;
    }
}
