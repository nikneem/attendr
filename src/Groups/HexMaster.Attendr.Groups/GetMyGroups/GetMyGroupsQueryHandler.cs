using HexMaster.Attendr.Core.CommandHandlers;

namespace HexMaster.Attendr.Groups.GetMyGroups;

/// <summary>
/// Query handler to retrieve all groups where the specified profile is a member.
/// Returns groups sorted alphabetically by name.
/// </summary>
public sealed class GetMyGroupsQueryHandler : IQueryHandler<GetMyGroupsQuery, IReadOnlyCollection<MyGroupDto>>
{
    private readonly IGroupRepository _groupRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMyGroupsQueryHandler"/> class.
    /// </summary>
    /// <param name="groupRepository">The repository for accessing group data.</param>
    public GetMyGroupsQueryHandler(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
    }

    /// <summary>
    /// Handles the GetMyGroupsQuery.
    /// </summary>
    /// <param name="query">The query containing the profile ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of groups where the user is a member, sorted alphabetically.</returns>
    public async Task<IReadOnlyCollection<MyGroupDto>> Handle(
        GetMyGroupsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Get all groups where the profile is a member
        var groups = await _groupRepository.GetGroupsByMemberIdAsync(query.ProfileId, cancellationToken);

        // Map to DTOs and sort alphabetically by name
        var result = groups
            .Select(g => new MyGroupDto(
                g.Id,
                g.Name,
                g.Members.Count))
            .OrderBy(g => g.Name)
            .ToList()
            .AsReadOnly();

        return result;
    }
}
