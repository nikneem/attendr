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
}
