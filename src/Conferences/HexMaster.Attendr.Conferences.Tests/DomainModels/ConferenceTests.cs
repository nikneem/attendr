using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Conferences.Tests.Factories;
using HexMaster.Attendr.Core.DomainModels;

namespace HexMaster.Attendr.Conferences.Tests.DomainModels;

public class ConferenceTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateConference()
    {
        // Arrange
        var title = "Test Conference";
        var city = "Amsterdam";
        var country = "Netherlands";
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
        var endDate = startDate.AddDays(3);
        var imageUrl = "https://test.com/image.jpg";

        // Act
        var conference = Conference.Create(title, city, country, startDate, endDate, imageUrl);

        // Assert
        Assert.NotNull(conference);
        Assert.NotEqual(Guid.Empty, conference.Id);
        Assert.Equal(title, conference.Title);
        Assert.Equal(city, conference.City);
        Assert.Equal(country, conference.Country);
        Assert.Equal(startDate, conference.StartDate);
        Assert.Equal(endDate, conference.EndDate);
        Assert.Equal(imageUrl, conference.ImageUrl);
        Assert.Equal(DomainModelState.Created, conference.State);
        Assert.Empty(conference.Rooms);
        Assert.Empty(conference.Speakers);
        Assert.Empty(conference.Presentations);
    }

    [Fact]
    public void Create_WithNullTitle_ShouldThrowArgumentNullException()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
        var endDate = startDate.AddDays(3);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            Conference.Create(null!, "City", "Country", startDate, endDate));
        Assert.Contains("title", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
        var endDate = startDate.AddDays(3);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Conference.Create(string.Empty, "City", "Country", startDate, endDate));
        Assert.Contains("title", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithNullCity_ShouldThrowArgumentNullException()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
        var endDate = startDate.AddDays(3);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            Conference.Create("Title", null!, "Country", startDate, endDate));
        Assert.Contains("city", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithEmptyCity_ShouldThrowArgumentException()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
        var endDate = startDate.AddDays(3);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Conference.Create("Title", string.Empty, "Country", startDate, endDate));
        Assert.Contains("city", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithNullCountry_ShouldThrowArgumentNullException()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
        var endDate = startDate.AddDays(3);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            Conference.Create("Title", "City", null!, startDate, endDate));
        Assert.Contains("country", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithEmptyCountry_ShouldThrowArgumentException()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
        var endDate = startDate.AddDays(3);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Conference.Create("Title", "City", string.Empty, startDate, endDate));
        Assert.Contains("country", exception.Message.ToLower());
    }

    [Fact]
    public void FromPersisted_WithValidData_ShouldCreateConference()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Conference";
        var city = "Amsterdam";
        var country = "Netherlands";
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
        var endDate = startDate.AddDays(3);

        // Act
        var conference = Conference.FromPersisted(id, title, city, country, startDate, endDate);

        // Assert
        Assert.NotNull(conference);
        Assert.Equal(id, conference.Id);
        Assert.Equal(title, conference.Title);
        Assert.Equal(DomainModelState.Pristine, conference.State);
    }

    [Fact]
    public void AddRoom_WithValidRoom_ShouldAddRoom()
    {
        // Arrange
        var conference = ConferenceFactory.CreateConference();
        var room = ConferenceFactory.CreateRoom();

        // Act
        conference.AddRoom(room);

        // Assert
        Assert.Single(conference.Rooms);
        Assert.Contains(room, conference.Rooms);
    }

    [Fact]
    public void AddRoom_WithNullRoom_ShouldThrowArgumentNullException()
    {
        // Arrange
        var conference = ConferenceFactory.CreateConference();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => conference.AddRoom(null!));
    }

    [Fact]
    public void AddRoom_WithDuplicateRoomId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var conference = ConferenceFactory.CreateConference();
        var room = ConferenceFactory.CreateRoom();
        conference.AddRoom(room);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => conference.AddRoom(room));
        Assert.Contains("already exists", exception.Message.ToLower());
    }

    [Fact]
    public void AddSpeaker_WithValidSpeaker_ShouldAddSpeaker()
    {
        // Arrange
        var conference = ConferenceFactory.CreateConference();
        var speaker = ConferenceFactory.CreateSpeaker();

        // Act
        conference.AddSpeaker(speaker);

        // Assert
        Assert.Single(conference.Speakers);
        Assert.Contains(speaker, conference.Speakers);
    }

    [Fact]
    public void AddSpeaker_WithNullSpeaker_ShouldThrowArgumentNullException()
    {
        // Arrange
        var conference = ConferenceFactory.CreateConference();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => conference.AddSpeaker(null!));
    }

    [Fact]
    public void AddPresentation_WithValidPresentation_ShouldAddPresentation()
    {
        // Arrange
        var conference = ConferenceFactory.CreateConference();
        var room = ConferenceFactory.CreateRoom();
        var speaker = ConferenceFactory.CreateSpeaker();
        conference.AddRoom(room);
        conference.AddSpeaker(speaker);

        // Create presentation with dates within conference dates
        var presentationStart = conference.StartDate.ToDateTime(new TimeOnly(10, 0));
        var presentationEnd = presentationStart.AddHours(1);
        var presentation = ConferenceFactory.CreatePresentation(
            startDateTime: presentationStart,
            endDateTime: presentationEnd,
            room: room,
            speakers: new[] { speaker });

        // Act
        conference.AddPresentation(presentation);

        // Assert
        Assert.Single(conference.Presentations);
        Assert.Contains(presentation, conference.Presentations);
    }

    [Fact]
    public void AddPresentation_WithNullPresentation_ShouldThrowArgumentNullException()
    {
        // Arrange
        var conference = ConferenceFactory.CreateConference();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => conference.AddPresentation(null!));
    }

    [Fact]
    public void AddPresentation_WithNonExistentRoom_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var conference = ConferenceFactory.CreateConference();
        var nonExistentRoom = ConferenceFactory.CreateRoom();
        var presentation = ConferenceFactory.CreatePresentation(room: nonExistentRoom);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => conference.AddPresentation(presentation));
        Assert.Contains("room", exception.Message.ToLower());
    }

    [Fact]
    public void AddPresentation_WithNonExistentSpeaker_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var conference = ConferenceFactory.CreateConference();
        var room = ConferenceFactory.CreateRoom();
        conference.AddRoom(room);

        var nonExistentSpeaker = ConferenceFactory.CreateSpeaker();
        var presentation = ConferenceFactory.CreatePresentation(
            room: room,
            speakers: new[] { nonExistentSpeaker });

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => conference.AddPresentation(presentation));
        Assert.Contains("speaker", exception.Message.ToLower());
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateConference()
    {
        // Arrange
        var conference = ConferenceFactory.CreateConference();
        var newTitle = "Updated Conference";
        var newCity = "Rotterdam";
        var newCountry = "Netherlands";
        var newStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(2));
        var newEndDate = newStartDate.AddDays(5);
        var newImageUrl = "https://updated.com/image.jpg";

        // Act
        conference.UpdateDetails(newTitle, newCity, newCountry, newStartDate, newEndDate, newImageUrl);

        // Assert
        Assert.Equal(newTitle, conference.Title);
        Assert.Equal(newCity, conference.City);
        Assert.Equal(newCountry, conference.Country);
        Assert.Equal(newStartDate, conference.StartDate);
        Assert.Equal(newEndDate, conference.EndDate);
        Assert.Equal(newImageUrl, conference.ImageUrl);
    }

    [Fact]
    public void UpdateDetails_WithNullTitle_ShouldThrowArgumentNullException()
    {
        // Arrange
        var conference = ConferenceFactory.CreateConference();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));
        var endDate = startDate.AddDays(3);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            conference.UpdateDetails(null!, "City", "Country", startDate, endDate));
        Assert.Contains("title", exception.Message.ToLower());
    }
}
