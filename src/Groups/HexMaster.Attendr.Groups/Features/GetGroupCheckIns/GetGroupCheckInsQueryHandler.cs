using System.Diagnostics;
using HexMaster.Attendr.Core.CommandHandlers;
using HexMaster.Attendr.Core.Observability;
using HexMaster.Attendr.Groups.Abstractions.Dtos;
using HexMaster.Attendr.Groups.Repositories;
using Microsoft.Extensions.Logging;

namespace HexMaster.Attendr.Groups.Features.GetGroupCheckIns;

/// <summary>
/// Query handler to retrieve all active check-ins for a specific group.
/// </summary>
public sealed class GetGroupCheckInsQueryHandler : IQueryHandler<GetGroupCheckInsQuery, IReadOnlyCollection<CheckInDto>>
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly ILogger<GetGroupCheckInsQueryHandler> _logger;

    public GetGroupCheckInsQueryHandler(
        ICheckInRepository checkInRepository,
        ILogger<GetGroupCheckInsQueryHandler> logger)
    {
        _checkInRepository = checkInRepository ?? throw new ArgumentNullException(nameof(checkInRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyCollection<CheckInDto>> Handle(GetGroupCheckInsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        using var activity = ActivitySources.Groups.StartActivity("GetGroupCheckIns", ActivityKind.Internal);
        activity?.SetTag("group.id", query.GroupId);

        try
        {
            var checkIns = await _checkInRepository.GetActiveByGroupAsync(query.GroupId, cancellationToken);

            activity?.SetTag("checkins.count", checkIns.Count);

            var dtos = checkIns.Select(checkIn => new CheckInDto(
                checkIn.Id,
                checkIn.GroupId,
                checkIn.ConferenceId,
                checkIn.PresentationId,
                new CheckInPresentationDataDto(
                    checkIn.PresentationData.Id,
                    checkIn.PresentationData.Title,
                    checkIn.PresentationData.Abstract,
                    checkIn.PresentationData.Room,
                    checkIn.PresentationData.StartDateTime,
                    checkIn.PresentationData.EndDateTime,
                    checkIn.PresentationData.Speakers.Select(s => new CheckInPresentationSpeakerDto(
                        s.Id,
                        s.Name,
                        s.ProfilePictureUrl
                    )).ToList()
                ),
                checkIn.Members.Select(m => new CheckedInMemberDto(
                    m.Id,
                    m.Name,
                    m.ProfilePictureUrl
                )).ToList(),
                checkIn.Expiration
            )).ToList();

            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(
                "Retrieved {Count} active check-ins for group {GroupId}",
                dtos.Count,
                query.GroupId);

            return dtos;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);

            _logger.LogError(ex,
                "Failed to retrieve active check-ins for group {GroupId}",
                query.GroupId);
            throw;
        }
    }
}
