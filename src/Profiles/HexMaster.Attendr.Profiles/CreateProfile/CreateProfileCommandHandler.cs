using HexMaster.Attendr.Core.Cache;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.DomainModels;
using HexMaster.Attendr.Profiles.Repositories;

namespace HexMaster.Attendr.Profiles.CreateProfile;

/// <summary>
/// Handler for the CreateProfileCommand.
/// Creates a new user profile if one does not already exist with the same SubjectId.
/// </summary>
public sealed class CreateProfileCommandHandler : ICommandHandler<CreateProfileCommand, CreateProfileResult>
{
    private readonly IProfileRepository _profileRepository;
    private readonly IAttendrCacheClient _cacheClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateProfileCommandHandler"/> class.
    /// </summary>
    /// <param name="profileRepository">The repository for managing profiles.</param>
    /// <param name="cacheClient">Cache client for storing resolved profile info.</param>
    public CreateProfileCommandHandler(IProfileRepository profileRepository, IAttendrCacheClient cacheClient)
    {
        _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
        _cacheClient = cacheClient ?? throw new ArgumentNullException(nameof(cacheClient));
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
            // Prime cache with resolved profile data
            var resolved = new ResolveProfileResult(existingProfile.Id, existingProfile.DisplayName);
            await _cacheClient.SetAsync(CacheKeys.Profiles.Subject(command.SubjectId), resolved, cancellationToken: cancellationToken);
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

        // Prime cache with newly created profile data
        var createdResolved = new ResolveProfileResult(profileId, profile.DisplayName);
        await _cacheClient.SetAsync(CacheKeys.Profiles.Subject(command.SubjectId), createdResolved, cancellationToken: cancellationToken);

        return new CreateProfileResult(profileId);
    }
}
