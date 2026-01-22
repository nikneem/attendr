using System.Diagnostics;
using HexMaster.Attendr.Core.Cache;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.DomainModels;
using HexMaster.Attendr.Profiles.Observability;
using HexMaster.Attendr.Profiles.Repositories;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Profiles.Features.CreateProfile;

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

            var existingProfile = await _profileRepository.GetBySubjectIdAsync(command.SubjectId, cancellationToken);
            if (existingProfile is not null)
            {
                _logger.LogInformation("Profile already exists for subject {SubjectId}, returning existing profile {ProfileId}",
                    command.SubjectId, existingProfile.Id);

                activity?.SetTag("profile.id", existingProfile.Id);
                activity?.SetTag("profile.action", "existing");

                var resolvedExisting = new ResolveProfileResult(existingProfile.Id, existingProfile.DisplayName);
                await _cacheClient.SetAsync(CacheKeys.Profiles.Subject(command.SubjectId), resolvedExisting, cancellationToken: cancellationToken);

                activity?.SetStatus(ActivityStatusCode.Ok);
                _metrics.RecordProfileExisting();
                _metrics.RecordOperationDuration("CreateProfile", stopwatch.Elapsed.TotalMilliseconds, success: true);

                return new CreateProfileResult(
                    existingProfile.Id,
                    existingProfile.FirstName ?? string.Empty,
                    existingProfile.LastName ?? string.Empty,
                    existingProfile.Email,
                    existingProfile.DisplayName);
            }

            var profile = Profile.Create(
                command.SubjectId,
                command.DisplayName,
                command.FirstName,
                command.LastName,
                command.Email);

            activity?.SetTag("profile.id", profile.Id);
            activity?.SetTag("profile.display_name", profile.DisplayName);
            activity?.SetTag("profile.action", "created");

            await _profileRepository.AddAsync(profile, cancellationToken);
            _logger.LogInformation("Profile created successfully with ID {ProfileId} for subject {SubjectId}",
                profile.Id, command.SubjectId);

            var resolved = new ResolveProfileResult(profile.Id, profile.DisplayName);
            await _cacheClient.SetAsync(CacheKeys.Profiles.Subject(command.SubjectId), resolved, cancellationToken: cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordProfileCreated();
            _metrics.RecordOperationDuration("CreateProfile", stopwatch.Elapsed.TotalMilliseconds, success: true);

            return new CreateProfileResult(
                profile.Id,
                profile.FirstName ?? string.Empty,
                profile.LastName ?? string.Empty,
                profile.Email,
                profile.DisplayName);
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
