using HexMaster.Attendr.Presence;
using HexMaster.Attendr.Presence.Data.MongoDb.Mappers;
using HexMaster.Attendr.Presence.Data.MongoDb.Models;
using HexMaster.Attendr.Presence.DomainModels;
using MongoDB.Driver;

namespace HexMaster.Attendr.Presence.Data.MongoDb;

/// <summary>
/// MongoDB-based repository implementation for conference presence aggregates.
/// </summary>
public sealed class ConferencePresenceRepository : IConferencePresenceRepository
{
    private readonly IMongoCollection<ConferencePresenceDocument> _collection;

    public ConferencePresenceRepository(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _collection = database.GetCollection<ConferencePresenceDocument>("conferencePresence");
    }

    public async Task<bool> ExistsAsync(Guid profileId, Guid conferenceId, CancellationToken cancellationToken = default)
    {
        var id = ConferencePresenceMapper.BuildId(profileId, conferenceId);
        var filter = Builders<ConferencePresenceDocument>.Filter.Eq(d => d.Id, id);
        var doc = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return doc is not null;
    }

    public async Task AddAsync(ConferencePresence presence, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(presence);
        var doc = ConferencePresenceMapper.ToDocument(presence);
        await _collection.InsertOneAsync(doc, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<ConferencePresence>> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ConferencePresenceDocument>.Filter.Eq(d => d.ProfileId, profileId);
        var docs = await _collection.Find(filter).ToListAsync(cancellationToken).ConfigureAwait(false);
        var presences = docs.Select(ConferencePresenceMapper.ToDomain).ToList();
        return presences.AsReadOnly();
    }

    public async Task<ConferencePresence?> GetAsync(Guid conferenceId, Guid profileId, CancellationToken cancellationToken = default)
    {
        var id = ConferencePresenceMapper.BuildId(profileId, conferenceId);
        var filter = Builders<ConferencePresenceDocument>.Filter.Eq(d => d.Id, id);
        var doc = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return doc is not null ? ConferencePresenceMapper.ToDomain(doc) : null;
    }

    public async Task UpdateAsync(ConferencePresence presence, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(presence);
        var doc = ConferencePresenceMapper.ToDocument(presence);
        var filter = Builders<ConferencePresenceDocument>.Filter.Eq(d => d.Id, doc.Id);
        await _collection.ReplaceOneAsync(filter, doc, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid conferenceId, Guid profileId, CancellationToken cancellationToken = default)
    {
        var id = ConferencePresenceMapper.BuildId(profileId, conferenceId);
        var filter = Builders<ConferencePresenceDocument>.Filter.Eq(d => d.Id, id);
        await _collection.DeleteOneAsync(filter, cancellationToken).ConfigureAwait(false);
    }
}
