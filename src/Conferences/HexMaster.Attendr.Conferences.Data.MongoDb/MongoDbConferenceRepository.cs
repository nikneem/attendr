using HexMaster.Attendr.Conferences.Abstractions.Dtos;
using HexMaster.Attendr.Conferences.Data.MongoDb.Mappers;
using HexMaster.Attendr.Conferences.Data.MongoDb.Models;
using HexMaster.Attendr.Conferences.DomainModels;
using MongoDB.Driver;

namespace HexMaster.Attendr.Conferences.Data.MongoDb;

/// <summary>
/// MongoDB implementation of IConferenceRepository.
/// </summary>
public sealed class MongoDbConferenceRepository : IConferenceRepository
{
    private readonly IMongoCollection<ConferenceDocument> _collection;

    public MongoDbConferenceRepository(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _collection = database.GetCollection<ConferenceDocument>("conferences");
    }

    /// <inheritdoc />
    public async Task AddAsync(Conference conference, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conference);

        var document = ConferenceMapper.ToDocument(conference);
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Conference?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ConferenceDocument>.Filter.Eq(c => c.Id, id);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return document != null ? ConferenceMapper.ToDomain(document) : null;
    }

    /// <inheritdoc />
    public async Task<ConferenceDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ConferenceDocument>.Filter.Eq(c => c.Id, id);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return document != null ? ConferenceDocumentToDtoMapper.ToDetailsDto(document) : null;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Conference conference, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conference);

        var filter = Builders<ConferenceDocument>.Filter.Eq(c => c.Id, conference.Id);
        var document = ConferenceMapper.ToDocument(conference);

        var result = await _collection.ReplaceOneAsync(filter, document, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException($"Conference with ID {conference.Id} does not exist.");
        }
    }

    /// <inheritdoc />
    public async Task<(List<Conference> Conferences, int TotalCount)> ListConferencesAsync(
        string? searchQuery,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        // Build filter
        var filterBuilder = Builders<ConferenceDocument>.Filter;
        var filter = filterBuilder.Gte(c => c.EndDate, today);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var searchFilter = filterBuilder.Or(
                filterBuilder.Regex(c => c.Title, new MongoDB.Bson.BsonRegularExpression(searchQuery, "i")),
                filterBuilder.Regex(c => c.City, new MongoDB.Bson.BsonRegularExpression(searchQuery, "i")),
                filterBuilder.Regex(c => c.Country, new MongoDB.Bson.BsonRegularExpression(searchQuery, "i"))
            );
            filter = filterBuilder.And(filter, searchFilter);
        }

        // Get total count
        var totalCount = (int)await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken).ConfigureAwait(false);

        // Get paginated results
        var documents = await _collection
            .Find(filter)
            .SortBy(c => c.StartDate)
            .ThenBy(c => c.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var conferences = documents.Select(ConferenceMapper.ToDomain).ToList();

        return (conferences, totalCount);
    }
}
