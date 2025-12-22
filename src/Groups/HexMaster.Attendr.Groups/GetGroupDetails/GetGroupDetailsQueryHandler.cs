using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Groups.GetGroupDetails;

/// <summary>
/// Query handler to retrieve detailed information about a specific group.
/// </summary>
public sealed class GetGroupDetailsQueryHandler : IQueryHandler<GetGroupDetailsQuery, GroupDetailsDto?>
{
    private readonly IGroupRepository _groupRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGroupDetailsQueryHandler"/> class.
    /// </summary>
    /// <param name="groupRepository">The repository for accessing group data.</param>
    public GetGroupDetailsQueryHandler(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
    }

    /// <summary>
    /// Handles the GetGroupDetailsQuery.
    /// </summary>
    /// <param name="query">The query containing the group ID and profile ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The group details if found; otherwise, null.</returns>
    public async Task<GroupDetailsDto?> Handle(
        GetGroupDetailsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Get group from repository
        var group = await _groupRepository.GetByIdAsync(query.GroupId, cancellationToken);

        if (group is null)
        {
            return null;
        }

        // Get current member role
        var currentMember = group.Members.FirstOrDefault(m => m.Id == query.ProfileId);
        var currentMemberRole = currentMember?.Role;

        // Map members to DTOs
        var members = group.Members
            .Select(m => new GetGroupDetailsMemberDto(m.Id, m.Name, m.Role))
            .ToList();

        // Map invitations to DTOs
        var invitations = group.Invitations
            .Select(i => new GroupInvitationDto(i.Id, i.Name, i.ExpirationDate))
            .ToList();

        // Map join requests to DTOs
        var joinRequests = group.JoinRequests
            .Select(jr => new GroupJoinRequestDto(jr.Id, jr.Name, jr.RequestedAt))
            .ToList();

        // Map to DTO with membership information
        return new GroupDetailsDto(
            group.Id,
            group.Name,
            group.Members.Count,
            currentMember != null,
            group.Settings.IsPublic,
            currentMemberRole,
            members,
            invitations,
            joinRequests);
    }
}
