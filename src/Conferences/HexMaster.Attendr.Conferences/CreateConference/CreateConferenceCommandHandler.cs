using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Conferences.CreateConference;

/// <summary>
/// Command handler to create a new conference.
/// </summary>
public sealed class CreateConferenceCommandHandler : ICommandHandler<CreateConferenceCommand, CreateConferenceResult>
{
    private readonly IConferenceRepository _conferenceRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateConferenceCommandHandler"/> class.
    /// </summary>
    /// <param name="conferenceRepository">The conference repository.</param>
    public CreateConferenceCommandHandler(IConferenceRepository conferenceRepository)
    {
        _conferenceRepository = conferenceRepository ?? throw new ArgumentNullException(nameof(conferenceRepository));
    }

    /// <summary>
    /// Handles the CreateConferenceCommand.
    /// </summary>
    /// <param name="command">The command containing conference details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created conference result.</returns>
    public async Task<CreateConferenceResult> Handle(
        CreateConferenceCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Create synchronization source if provided
        SynchronizationSource? syncSource = null;
        if (command.SynchronizationSource is not null)
        {
            if (!Enum.TryParse<SynchronizationSourceType>(command.SynchronizationSource.SourceType, true, out var sourceType))
            {
                throw new ArgumentException($"Invalid synchronization source type: {command.SynchronizationSource.SourceType}");
            }

            syncSource = SynchronizationSource.CreateWithUrl(sourceType, command.SynchronizationSource.SourceUrl);
        }

        // Create the conference
        var conference = Conference.Create(
            command.Title,
            command.City,
            command.Country,
            command.StartDate,
            command.EndDate,
            null, // ImageUrl - not provided when creating from command
            syncSource);

        // Persist the conference
        await _conferenceRepository.AddAsync(conference, cancellationToken);

        // Return result
        return new CreateConferenceResult(
            conference.Id,
            conference.Title,
            conference.City,
            conference.Country,
            conference.StartDate,
            conference.EndDate);
    }
}
