using Bogus;
using HexMaster.Attendr.Presence.DomainModels;

namespace HexMaster.Attendr.Presence.Tests.Factories;

/// <summary>
/// Factory for creating test instances of ConferencePresence.
/// </summary>
public static class ConferencePresenceFactory
{
    private static readonly Faker _faker = new();

    public static ConferencePresence Create(
        Guid? conferenceId = null,
        string? conferenceName = null,
        string? location = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        Guid? profileId = null,
        string? imageUrl = null,
        bool isFollowing = true,
        bool isAttending = false,
        IEnumerable<PresentationPresence>? presentations = null)
    {
        var confId = conferenceId ?? Guid.NewGuid();
        var confName = conferenceName ?? _faker.Company.CompanyName() + " Conference";
        var loc = location ?? _faker.Address.City();
        var start = startDate ?? DateOnly.FromDateTime(_faker.Date.Future());
        var end = endDate ?? start.AddDays(_faker.Random.Int(1, 5));
        var profId = profileId ?? Guid.NewGuid();
        var img = imageUrl ?? _faker.Internet.Avatar();

        return new ConferencePresence(
            confId,
            confName,
            loc,
            start,
            end,
            profId,
            img,
            isFollowing,
            isAttending,
            presentations);
    }

    public static List<ConferencePresence> CreateList(
        int count,
        Guid? profileId = null,
        bool futureOnly = false)
    {
        var profId = profileId ?? Guid.NewGuid();
        var conferences = new List<ConferencePresence>();
        var now = DateTime.UtcNow;

        for (int i = 0; i < count; i++)
        {
            DateOnly startDate;
            if (futureOnly)
            {
                startDate = DateOnly.FromDateTime(_faker.Date.Future(refDate: now));
            }
            else
            {
                // Mix of past, current, and future
                var daysOffset = _faker.Random.Int(-30, 60);
                startDate = DateOnly.FromDateTime(now.AddDays(daysOffset));
            }

            var endDate = startDate.AddDays(_faker.Random.Int(1, 5));

            conferences.Add(Create(
                profileId: profId,
                startDate: startDate,
                endDate: endDate,
                isFollowing: true,
                isAttending: _faker.Random.Bool()));
        }

        return conferences;
    }
}
