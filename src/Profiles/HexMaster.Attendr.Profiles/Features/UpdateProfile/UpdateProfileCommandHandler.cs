using System.Diagnostics;
using HexMaster.Attendr.Core.Cache;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Profiles.Abstractions.Dtos;
using HexMaster.Attendr.Profiles.Observability;
using HexMaster.Attendr.Profiles.Repositories;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Profiles.Features.UpdateProfile;

public sealed class UpdateProfileCommandHandler : ICommandHandler<UpdateProfileCommand, UpdateProfileResult>
{
    private readonly IProfileRepository _profileRepository;
    private readonly IAttendrCacheClient _cacheClient;
    private readonly ProfileMetrics _metrics;
    private readonly ILogger<UpdateProfileCommandHandler> _logger;

    public UpdateProfileCommandHandler(
        IProfileRepository profileRepository,
        IAttendrCacheClient cacheClient,
        ProfileMetrics metrics,
        ILogger<UpdateProfileCommandHandler> logger)
    {
        _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
        _cacheClient = cacheClient ?? throw new ArgumentNullException(nameof(cacheClient));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UpdateProfileResult> Handle(UpdateProfileCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.SubjectId, nameof(command.SubjectId));

        using var activity = ActivitySources.Profiles.StartActivity("UpdateProfile", ActivityKind.Internal);
        activity?.SetTag("profile.subject_id", command.SubjectId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Updating profile for subject {SubjectId}", command.SubjectId);

            var profile = await _profileRepository.GetBySubjectIdAsync(command.SubjectId, cancellationToken);
            if (profile is null)
            {
                _metrics.RecordOperationFailed("UpdateProfile", "ProfileNotFound");
                throw new InvalidOperationException($"Profile for subject '{command.SubjectId}' was not found.");
            }

            profile.SetDisplayName(command.DisplayName);
            profile.SetFirstName(command.FirstName);
            profile.SetLastName(command.LastName);
            profile.SetTagLine(command.TagLine);
            profile.SetIsSearchable(command.IsSearchable);

            await _profileRepository.UpdateAsync(profile, cancellationToken);

            var resolved = new ResolveProfileResult(profile.Id, profile.DisplayName);
            await _cacheClient.SetAsync(CacheKeys.Profiles.Subject(profile.SubjectId), resolved, cancellationToken: cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            _metrics.RecordOperationDuration("UpdateProfile", stopwatch.Elapsed.TotalMilliseconds, success: true);

            return new UpdateProfileResult(
                profile.Id,
                profile.DisplayName,
                profile.FirstName ?? string.Empty,
                profile.LastName ?? string.Empty,
                profile.TagLine,
                profile.IsSearchable);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            _metrics.RecordOperationFailed("UpdateProfile", ex.GetType().Name);
            _metrics.RecordOperationDuration("UpdateProfile", stopwatch.Elapsed.TotalMilliseconds, success: false);
            _logger.LogError(ex, "Failed to update profile for subject {SubjectId}", command.SubjectId);
            throw;
        }
    }
}
