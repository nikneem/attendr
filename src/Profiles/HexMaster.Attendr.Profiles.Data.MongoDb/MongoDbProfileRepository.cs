using HexMaster.Attendr.Profiles.Data.MongoDb.Mappers;
using HexMaster.Attendr.Profiles.Data.MongoDb.Models;
using HexMaster.Attendr.Profiles.DomainModels;
using HexMaster.Attendr.Profiles.Repositories;
using MongoDB.Driver;

namespace HexMaster.Attendr.Profiles.Data.MongoDb;

/// <summary>
/// MongoDB implementation of IProfileRepository.
/// </summary>
public sealed class MongoDbProfileRepository : IProfileRepository
{
    private readonly IMongoCollection<ProfileDocument> _collection;

    public MongoDbProfileRepository(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _collection = database.GetCollection<ProfileDocument>("profiles");
    }

    /// <inheritdoc />
    public async Task<Profile?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ProfileDocument>.Filter.Eq(p => p.Id, id);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return document != null ? ProfileMapper.ToDomain(document) : null;
    }

    /// <inheritdoc />
    public async Task<Profile?> GetBySubjectIdAsync(string subjectId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ProfileDocument>.Filter.Eq(p => p.SubjectId, subjectId);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return document != null ? ProfileMapper.ToDomain(document) : null;
    }

    /// <inheritdoc />
    public async Task AddAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var document = ProfileMapper.ToDocument(profile);
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var filter = Builders<ProfileDocument>.Filter.Eq(p => p.Id, profile.Id);
        var document = ProfileMapper.ToDocument(profile);

        var result = await _collection.ReplaceOneAsync(filter, document, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException($"Profile with ID '{profile.Id}' was not found.");
        }
    }
}
