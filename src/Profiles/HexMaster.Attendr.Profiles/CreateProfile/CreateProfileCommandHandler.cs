using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.DomainModels;

namespace HexMaster.Attendr.Profiles.CreateProfile;

/// <summary>
/// Handler for the CreateProfileCommand.
/// Creates a new user profile if one does not already exist with the same SubjectId.
/// </summary>
public sealed class CreateProfileCommandHandler : ICommandHandler<CreateProfileCommand, CreateProfileResult>
{
    private readonly IProfileRepository _profileRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateProfileCommandHandler"/> class.
    /// </summary>
    /// <param name="profileRepository">The repository for managing profiles.</param>
    public CreateProfileCommandHandler(IProfileRepository profileRepository)
    {
        _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
    }

    /// <summary>
    /// Handles the CreateProfileCommand.
    /// </summary>
    /// <param name="command">The command containing profile creation data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation that returns the created profile result.</returns>
    public async Task<CreateProfileResult> Handle(CreateProfileCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.SubjectId, nameof(command.SubjectId));

        // Check if a profile with this SubjectId already exists
        var existingProfile = await _profileRepository.GetBySubjectIdAsync(command.SubjectId, cancellationToken);
        if (existingProfile is not null)
        {
            return new CreateProfileResult(existingProfile.Id);
        }

        // Create a new profile
        var profileId = Guid.NewGuid().ToString();
        var profile = new Profile(
            profileId,
            command.SubjectId,
            command.DisplayName,
            command.FirstName,
            command.LastName,
            command.Email,
            null,
            null,
            true,
            false
        );

        // Save the profile
        await _profileRepository.AddAsync(profile, cancellationToken);

        return new CreateProfileResult(profileId);
    }
}

/// <summary>
/// Repository interface for Profile aggregate root operations.
/// </summary>
public interface IProfileRepository
{
    /// <summary>
    /// Gets a profile by its unique identifier.
    /// </summary>
    /// <param name="id">The profile ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The profile if found; otherwise, null.</returns>
    Task<Profile?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a profile by its subject ID (from the authentication provider).
    /// </summary>
    /// <param name="subjectId">The subject ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The profile if found; otherwise, null.</returns>
    Task<Profile?> GetBySubjectIdAsync(string subjectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new profile to the repository.
    /// </summary>
    /// <param name="profile">The profile to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(Profile profile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing profile in the repository.
    /// </summary>
    /// <param name="profile">The profile to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(Profile profile, CancellationToken cancellationToken = default);
}
