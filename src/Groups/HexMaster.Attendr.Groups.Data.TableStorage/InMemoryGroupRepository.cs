using HexMaster.Attendr.Groups.DomainModels;

namespace HexMaster.Attendr.Groups.Data.TableStorage;

/// <summary>
/// In-memory implementation of IGroupRepository for development and testing.
/// TODO: Replace with persistent storage implementation (Azure Table Storage, etc.)
/// </summary>
public sealed class InMemoryGroupRepository : IGroupRepository
{
    private readonly Dictionary<Guid, Group> _groups = new();

    /// <inheritdoc />
    public Task AddAsync(Group group, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);
        _groups[group.Id] = group;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _groups.TryGetValue(id, out var group);
        return Task.FromResult(group);
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Group>> GetGroupsByMemberIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var groups = _groups.Values
            .Where(g => g.Members.Any(m => m.Id == memberId))
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyCollection<Group>>(groups);
    }

    /// <inheritdoc />
    public Task<(IReadOnlyCollection<Group> Groups, int TotalCount)> ListGroupsAsync(
        string? searchQuery,
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default)
    {
        var query = _groups.Values.AsEnumerable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var searchTerm = searchQuery.Trim().ToLowerInvariant();
            query = query.Where(g => g.Name.ToLowerInvariant().Contains(searchTerm));
        }

        var totalCount = query.Count();

        // Apply pagination
        var groups = query
            .OrderBy(g => g.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<(IReadOnlyCollection<Group> Groups, int TotalCount)>((groups, totalCount));
    }
}
