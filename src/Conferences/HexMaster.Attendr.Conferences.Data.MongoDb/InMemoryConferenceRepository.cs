using HexMaster.Attendr.Conferences.DomainModels;

namespace HexMaster.Attendr.Conferences.Data.MongoDb;

/// <summary>
/// In-memory implementation of IConferenceRepository for development and testing.
/// TODO: Replace with persistent storage implementation (MongoDB, etc.)
/// </summary>
public sealed class InMemoryConferenceRepository : IConferenceRepository
{
    private readonly Dictionary<Guid, Conference> _conferences = new();

    /// <inheritdoc />
    public Task AddAsync(Conference conference, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conference);
        _conferences[conference.Id] = conference;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<Conference?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _conferences.TryGetValue(id, out var conference);
        return Task.FromResult(conference);
    }

    /// <inheritdoc />
    public Task UpdateAsync(Conference conference, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conference);

        if (!_conferences.ContainsKey(conference.Id))
        {
            throw new InvalidOperationException($"Conference with ID {conference.Id} does not exist.");
        }

        _conferences[conference.Id] = conference;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<(List<Conference> Conferences, int TotalCount)> ListConferencesAsync(
        string? searchQuery,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = _conferences.Values
            .Where(c => c.EndDate >= today);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var search = searchQuery.ToLowerInvariant();
            query = query.Where(c =>
                c.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (c.City != null && c.City.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (c.Country != null && c.Country.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        var totalCount = query.Count();

        var conferences = query
            .OrderBy(c => c.StartDate)
            .ThenBy(c => c.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult((conferences, totalCount));
    }
}
