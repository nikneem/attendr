using Bogus;
using HexMaster.Attendr.Presence.DomainModels;

namespace HexMaster.Attendr.Presence.Tests.Factories;

/// <summary>
/// Factory for creating test instances of PresentationPresence.
/// </summary>
public static class PresentationPresenceFactory
{
    private static readonly Faker _faker = new();

    public static PresentationPresence Create(
        Guid? profileId = null,
        Guid? conferenceId = null,
        Guid? presentationId = null,
        string? title = null,
        string? @abstract = null,
        string? room = null,
        DateTimeOffset? startDateTime = null,
        DateTimeOffset? endDateTime = null,
        IEnumerable<PresentationSpeaker>? speakers = null,
        IEnumerable<PresentationTopic>? topics = null,
        bool isRated = false,
        bool isFavorite = false,
        bool isCheckedIn = false,
        DateTimeOffset? checkedInAt = null,
        byte? rating = null,
        bool isRecommended = false,
        bool isPreferred = false)
    {
        var start = startDateTime ?? DateTimeOffset.UtcNow.AddHours(1);
        var end = endDateTime ?? start.AddHours(1);

        return new PresentationPresence(
            profileId ?? Guid.NewGuid(),
            conferenceId ?? Guid.NewGuid(),
            presentationId ?? Guid.NewGuid(),
            title ?? _faker.Lorem.Sentence(3),
            @abstract ?? _faker.Lorem.Paragraph(),
            room ?? _faker.Commerce.Department(),
            start,
            end,
            speakers,
            topics,
            isRated,
            isFavorite,
            isCheckedIn,
            checkedInAt,
            rating,
            isRecommended,
            isPreferred);
    }

    public static List<PresentationPresence> CreateList(
        int count,
        Guid? profileId = null,
        Guid? conferenceId = null,
        bool asFavorite = false)
    {
        var profId = profileId ?? Guid.NewGuid();
        var confId = conferenceId ?? Guid.NewGuid();
        var start = DateTimeOffset.UtcNow.AddHours(1);

        return Enumerable.Range(0, count)
            .Select(i => Create(
                profileId: profId,
                conferenceId: confId,
                startDateTime: start.AddHours(i * 2),
                endDateTime: start.AddHours(i * 2 + 1),
                isFavorite: asFavorite))
            .ToList();
    }
}
