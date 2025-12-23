using System.Diagnostics;
using HexMaster.Attendr.Core.Cache;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.DomainModels;
using HexMaster.Attendr.Profiles.Observability;
using HexMaster.Attendr.Profiles.Repositories;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace HexMaster.Attendr.Profiles.CreateProfile;

/// <summary>
/// Handler for the CreateProfileCommand.
/// Creates a new user profile if one does not already exist with the same SubjectId.
/// Implements distributed tracing via OpenTelemetry and structured logging.
/// </summary>
public sealed class CreateProfileCommandHandler : ICommandHandler<CreateProfileCommand, CreateProfileResult>
{
    private readonly IProfileRepository _profileRepository;
    private readonly IAttendrCacheClient _cacheClient;
    private readonly ProfileMetrics _metrics;
    private readonly ILogger<CreateProfileCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateProfileCommandHandler"/> class.
    /// </summary>
    /// <param name="profileRepository">The repository for managing profiles.</param>
    /// <param name="cacheClient">Cache client for storing resolved profile info.</param>
    /// <param name="metrics">The metrics for recording profile operations.</param>
    /// <param name="logger">Logger for recording operation details and errors.</param>
    public CreateProfileCommandHandler(
        IProfileRepository profileRepository,
        IAttendrCacheClient cacheClient,
        ProfileMetrics metrics,
        ILogger<CreateProfileCommandHandler> logger)
    {
        _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
        _cacheClient = cacheClient ?? throw new ArgumentNullException(nameof(cacheClient));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        using var activity = ActivitySources.Profiles.StartActivity("CreateProfile", ActivityKind.Internal);
        activity?.SetTag("profile.subject_id", command.SubjectId);
        activity?.SetTag("profile.email", command.Email);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Attempting to create profile for subject {SubjectId}", command.SubjectId);

            // Check if a profile with this SubjectId already exists
            var existingProfile = await _profileRepository.GetBySubjectIdAsync(command.SubjectId, cancellationToken);
            if (existingProfile is not null)
            {
                _logger.LogInformation("Profile already exists for subject {SubjectId}, returning existing profile {ProfileId}",
                    command.SubjectId, existingProfile.Id);

                activity?.SetTag("profile.id", existingProfile.Id);
                activity?.SetTag("profile.action", "existing");

                // Prime cache with resolved profile data
                var resolved = new ResolveProfileResult(existingProfile.Id, existingProfile.DisplayName);
                await _cacheClient.SetAsync(CacheKeys.Profiles.Subject(command.SubjectId), resolved, cancellationToken: cancellationToken);

                activity?.SetStatus(ActivityStatusCode.Ok);
                _metrics.RecordProfileExisting();
                _metrics.RecordOperationDuration("CreateProfile", stopwatch.Elapsed.TotalMilliseconds, success: true);

                return new CreateProfileResult(existingProfile.Id);
            }

            // Create a new profile using factory method (domain model enforces business rules)
            var profile = Profile.Create(
                command.SubjectId,
                command.DisplayName,
                command.FirstName,
                command.LastName,
                command.Email
            );

            activity?.SetTag("profile.id", profile.Id);
            activity?.SetTag("profile.display_name", profile.DisplayName);
            activity?.SetTag("profile.action", "created");

            // Save the profile
            await _profileRepository.AddAsync(profile, cancellationToken);
            _logger.LogInformation("Profile created successfully with ID {ProfileId} for subject {SubjectId}",
                profile.Id, command.SubjectId);

            // Prime cache with newly created profile data
            var createdResolved = new ResolveProfileResult(profile.Id, profile.DisplayName);
            await _cacheClient.SetAsync(CacheKeys.Profiles.Subject(command.SubjectId), createdResolved, cancellationToken: cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordProfileCreated();
            _metrics.RecordOperationDuration("CreateProfile", stopwatch.Elapsed.TotalMilliseconds, success: true);

            return new CreateProfileResult(profile.Id);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("CreateProfile", ex.GetType().Name);
            _metrics.RecordOperationDuration("CreateProfile", stopwatch.Elapsed.TotalMilliseconds, success: false);

            _logger.LogError(ex, "Failed to create or retrieve profile for subject {SubjectId}", command.SubjectId);
            throw;
        }
    }
}
