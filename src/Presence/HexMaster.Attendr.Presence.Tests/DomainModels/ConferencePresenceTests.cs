using Bogus;
using HexMaster.Attendr.Presence.DomainModels;

namespace HexMaster.Attendr.Presence.Tests.DomainModels;

public sealed class ConferencePresenceTests
{
    private readonly Faker _faker;

    public ConferencePresenceTests()
    {
        _faker = new Faker();
    }

    [Fact]
    public void Create_WithValidParameters_ShouldCreateConferencePresence()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        var conferenceName = _faker.Company.CompanyName();
        var location = _faker.Address.City();
        var startDate = DateOnly.FromDateTime(_faker.Date.Future());
        var endDate = startDate.AddDays(3);
        var profileId = Guid.NewGuid();
        var imageUrl = _faker.Internet.Avatar();

        // Act
        var conference = new ConferencePresence(
            conferenceId,
            conferenceName,
            location,
            startDate,
            endDate,
            profileId,
            imageUrl,
            isFollowing: true,
            isAttending: false);

        // Assert
        Assert.Equal(conferenceId, conference.ConferenceId);
        Assert.Equal(conferenceName, conference.ConferenceName);
        Assert.Equal(location, conference.Location);
        Assert.Equal(startDate, conference.StartDate);
        Assert.Equal(endDate, conference.EndDate);
        Assert.Equal(profileId, conference.ProfileId);
        Assert.Equal(imageUrl, conference.ImageUrl);
        Assert.True(conference.IsFollowing);
        Assert.False(conference.IsAttending);
    }

    [Fact]
    public void Create_WithEmptyConferenceId_ShouldThrowArgumentException()
    {
        // Arrange
        var conferenceName = _faker.Company.CompanyName();
        var location = _faker.Address.City();
        var startDate = DateOnly.FromDateTime(_faker.Date.Future());
        var endDate = startDate.AddDays(3);
        var profileId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new ConferencePresence(
                Guid.Empty,
                conferenceName,
                location,
                startDate,
                endDate,
                profileId));
    }

    [Fact]
    public void Create_WithNullOrEmptyConferenceName_ShouldThrowArgumentException()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        var location = _faker.Address.City();
        var startDate = DateOnly.FromDateTime(_faker.Date.Future());
        var endDate = startDate.AddDays(3);
        var profileId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new ConferencePresence(
                conferenceId,
                string.Empty,
                location,
                startDate,
                endDate,
                profileId));
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_ShouldThrowArgumentException()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        var conferenceName = _faker.Company.CompanyName();
        var location = _faker.Address.City();
        var startDate = DateOnly.FromDateTime(_faker.Date.Future());
        var endDate = startDate.AddDays(-1); // End before start
        var profileId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new ConferencePresence(
                conferenceId,
                conferenceName,
                location,
                startDate,
                endDate,
                profileId));
    }

    [Fact]
    public void Create_WithEmptyProfileId_ShouldThrowArgumentException()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        var conferenceName = _faker.Company.CompanyName();
        var location = _faker.Address.City();
        var startDate = DateOnly.FromDateTime(_faker.Date.Future());
        var endDate = startDate.AddDays(3);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new ConferencePresence(
                conferenceId,
                conferenceName,
                location,
                startDate,
                endDate,
                Guid.Empty));
    }

    [Fact]
    public void Presentations_ShouldBeReadOnly()
    {
        // Arrange & Act
        var startDate = DateOnly.FromDateTime(_faker.Date.Future());
        var endDate = startDate.AddDays(3);

        var conference = new ConferencePresence(
            Guid.NewGuid(),
            _faker.Company.CompanyName(),
            _faker.Address.City(),
            startDate,
            endDate,
            Guid.NewGuid());

        // Assert
        Assert.NotNull(conference.Presentations);
        Assert.Empty(conference.Presentations);
        Assert.IsAssignableFrom<IReadOnlyCollection<PresentationPresence>>(conference.Presentations);
    }
}
