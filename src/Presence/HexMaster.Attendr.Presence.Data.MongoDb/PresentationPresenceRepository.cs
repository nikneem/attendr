using HexMaster.Attendr.Presence;
using HexMaster.Attendr.Presence.Data.MongoDb.Mappers;
using HexMaster.Attendr.Presence.Data.MongoDb.Models;
using HexMaster.Attendr.Presence.DomainModels;
using MongoDB.Driver;

namespace HexMaster.Attendr.Presence.Data.MongoDb;

/// <summary>
/// MongoDB-based repository implementation for presentation presence operations.
/// </summary>
public sealed class PresentationPresenceRepository : IPresentationPresenceRepository
{
    private readonly IMongoCollection<PresentationPresenceDocument> _collection;

    public PresentationPresenceRepository(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _collection = database.GetCollection<PresentationPresenceDocument>("presentationPresence");
    }

    public async Task<IReadOnlyCollection<PresentationPresence>> GetByConferenceAndPresentationAsync(
        Guid conferenceId,
        Guid presentationId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<PresentationPresenceDocument>.Filter.And(
            Builders<PresentationPresenceDocument>.Filter.Eq(d => d.ConferenceId, conferenceId),
            Builders<PresentationPresenceDocument>.Filter.Eq(d => d.PresentationId, presentationId));

        var docs = await _collection.Find(filter).ToListAsync(cancellationToken).ConfigureAwait(false);
        var presentations = docs.Select(PresentationPresenceMapper.ToDomain).ToList();
        return presentations.AsReadOnly();
    }

    public async Task<PresentationPresence?> GetByIdAsync(
        Guid profileId,
        Guid conferenceId,
        Guid presentationId,
        CancellationToken cancellationToken = default)
    {
        var id = PresentationPresenceMapper.BuildId(profileId, conferenceId, presentationId);
        var filter = Builders<PresentationPresenceDocument>.Filter.Eq(d => d.Id, id);
        var doc = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return doc != null ? PresentationPresenceMapper.ToDomain(doc) : null;
    }

    public async Task<IReadOnlyCollection<PresentationPresence>> GetUnratedByProfileAndConferenceAsync(
        Guid profileId,
        Guid conferenceId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<PresentationPresenceDocument>.Filter.And(
            Builders<PresentationPresenceDocument>.Filter.Eq(d => d.ProfileId, profileId),
            Builders<PresentationPresenceDocument>.Filter.Eq(d => d.ConferenceId, conferenceId),
            Builders<PresentationPresenceDocument>.Filter.Eq(d => d.IsRated, false));

        var docs = await _collection.Find(filter).ToListAsync(cancellationToken).ConfigureAwait(false);
        var presentations = docs.Select(PresentationPresenceMapper.ToDomain).ToList();
        return presentations.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<PresentationPresence>> GetByProfileAndConferenceAsync(
        Guid profileId,
        Guid conferenceId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<PresentationPresenceDocument>.Filter.And(
            Builders<PresentationPresenceDocument>.Filter.Eq(d => d.ProfileId, profileId),
            Builders<PresentationPresenceDocument>.Filter.Eq(d => d.ConferenceId, conferenceId));

        var docs = await _collection.Find(filter).ToListAsync(cancellationToken).ConfigureAwait(false);
        var presentations = docs.Select(PresentationPresenceMapper.ToDomain).ToList();
        return presentations.AsReadOnly();
    }

    public async Task UpdateAsync(
        Guid profileId,
        Guid conferenceId,
        PresentationPresence presentation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(presentation);

        var id = PresentationPresenceMapper.BuildId(profileId, conferenceId, presentation.PresentationId);
        var filter = Builders<PresentationPresenceDocument>.Filter.Eq(d => d.Id, id);
        var doc = PresentationPresenceMapper.ToDocument(profileId, conferenceId, presentation);

        await _collection.ReplaceOneAsync(filter, doc, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(
        Guid profileId,
        Guid conferenceId,
        Guid presentationId,
        CancellationToken cancellationToken = default)
    {
        var id = PresentationPresenceMapper.BuildId(profileId, conferenceId, presentationId);
        var filter = Builders<PresentationPresenceDocument>.Filter.Eq(d => d.Id, id);
        await _collection.DeleteOneAsync(filter, cancellationToken).ConfigureAwait(false);
    }
}
