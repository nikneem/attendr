using HexMaster.Attendr.Groups.Data.MongoDb.Mappers;
using HexMaster.Attendr.Groups.Data.MongoDb.Models;
using HexMaster.Attendr.Groups.DomainModels;
using MongoDB.Driver;

namespace HexMaster.Attendr.Groups.Data.MongoDb;

/// <summary>
/// MongoDB implementation of IGroupRepository.
/// </summary>
public sealed class MongoDbGroupRepository : IGroupRepository
{
    private readonly IMongoCollection<GroupDocument> _collection;

    public MongoDbGroupRepository(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _collection = database.GetCollection<GroupDocument>("groups");
    }

    /// <inheritdoc />
    public async Task AddAsync(Group group, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        var document = GroupMapper.ToDocument(group);
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Group group, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(group);

        var document = GroupMapper.ToDocument(group);
        var filter = Builders<GroupDocument>.Filter.Eq(g => g.Id, group.Id);
        await _collection.ReplaceOneAsync(filter, document, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<GroupDocument>.Filter.Eq(g => g.Id, id);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return document != null ? GroupMapper.ToDomain(document) : null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Group>> GetGroupsByMemberIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<GroupDocument>.Filter.ElemMatch(
            g => g.Members,
            m => m.Id == memberId);

        var documents = await _collection
            .Find(filter)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var groups = documents.Select(GroupMapper.ToDomain).ToList();
        return groups.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyCollection<Group> Groups, int TotalCount)> ListGroupsAsync(
        string? searchQuery,
        int pageSize,
        int pageNumber,
        CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<GroupDocument>.Filter;

        // Only include searchable groups
        var filter = filterBuilder.Eq(g => g.Settings.IsSearchable, true);

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var searchFilter = filterBuilder.Regex(
                g => g.Name,
                new MongoDB.Bson.BsonRegularExpression(searchQuery, "i"));
            filter = filterBuilder.And(filter, searchFilter);
        }

        // Get total count
        var totalCount = (int)await _collection
            .CountDocumentsAsync(filter, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // Get paginated results
        var documents = await _collection
            .Find(filter)
            .SortBy(g => g.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var groups = documents.Select(GroupMapper.ToDomain).ToList();

        return (groups.AsReadOnly(), totalCount);
    }
}
