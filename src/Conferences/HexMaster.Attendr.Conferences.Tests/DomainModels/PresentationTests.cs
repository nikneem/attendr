using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Core.DomainModels;

namespace HexMaster.Attendr.Conferences.Tests.DomainModels;

public class PresentationTests
{
    private static Speaker CreateTestSpeaker() => Speaker.Create("Test Speaker");
    private static Room CreateTestRoom() => Room.Create("Test Room", 100);

    [Fact]
    public void Create_WithValidData_ShouldCreatePresentation()
    {
        // Arrange
        var title = "Introduction to .NET";
        var abstractText = "Learn about .NET fundamentals";
        var startDateTime = DateTimeOffset.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);
        var room = CreateTestRoom();
        var speakers = new[] { CreateTestSpeaker() };

        // Act
        var presentation = Presentation.Create(title, abstractText, startDateTime, endDateTime, room, speakers);

        // Assert
        Assert.NotNull(presentation);
        Assert.NotEqual(Guid.Empty, presentation.Id);
        Assert.Equal(title, presentation.Title);
        Assert.Equal(abstractText, presentation.Abstract);
        Assert.Equal(startDateTime, presentation.StartDateTime);
        Assert.Equal(endDateTime, presentation.EndDateTime);
        Assert.Equal(room.Id, presentation.Room.Id);
        Assert.Single(presentation.Speakers);
        Assert.Equal(DomainModelState.Created, presentation.State);
    }

    [Fact]
    public void Create_WithExternalId_ShouldCreatePresentationWithExternalId()
    {
        // Arrange
        var title = "Introduction to .NET";
        var abstractText = "Learn about .NET fundamentals";
        var startDateTime = DateTimeOffset.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);
        var room = CreateTestRoom();
        var speakers = new[] { CreateTestSpeaker() };
        var externalId = "ext-789";

        // Act
        var presentation = Presentation.Create(title, abstractText, startDateTime, endDateTime, room, speakers, externalId);

        // Assert
        Assert.Equal(externalId, presentation.ExternalId);
    }

    [Fact]
    public void Create_WithNullTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var startDateTime = DateTimeOffset.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Presentation.Create(null!, "Abstract", startDateTime, endDateTime, CreateTestRoom(), new[] { CreateTestSpeaker() }));
        Assert.Contains("title", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var startDateTime = DateTimeOffset.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Presentation.Create(string.Empty, "Abstract", startDateTime, endDateTime, CreateTestRoom(), new[] { CreateTestSpeaker() }));
        Assert.Contains("title", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithNullAbstract_ShouldThrowArgumentException()
    {
        // Arrange
        var startDateTime = DateTimeOffset.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Presentation.Create("Title", null!, startDateTime, endDateTime, CreateTestRoom(), new[] { CreateTestSpeaker() }));
        Assert.Contains("abstract", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithEmptyAbstract_ShouldThrowArgumentException()
    {
        // Arrange
        var startDateTime = DateTimeOffset.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Presentation.Create("Title", string.Empty, startDateTime, endDateTime, CreateTestRoom(), new[] { CreateTestSpeaker() }));
        Assert.Contains("abstract", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_ShouldThrowArgumentException()
    {
        // Arrange
        var startDateTime = DateTimeOffset.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(-1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Presentation.Create("Title", "Abstract", startDateTime, endDateTime, CreateTestRoom(), new[] { CreateTestSpeaker() }));
        Assert.Contains("end", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithNullRoom_ShouldThrowArgumentNullException()
    {
        // Arrange
        var startDateTime = DateTimeOffset.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            Presentation.Create("Title", "Abstract", startDateTime, endDateTime, null!, new[] { CreateTestSpeaker() }));
        Assert.Equal("room", exception.ParamName);
    }

    [Fact]
    public void Create_WithNullSpeakers_ShouldThrowArgumentNullException()
    {
        // Arrange
        var startDateTime = DateTimeOffset.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            Presentation.Create("Title", "Abstract", startDateTime, endDateTime, CreateTestRoom(), null!));
        Assert.Equal("speakers", exception.ParamName);
    }

    [Fact]
    public void Create_WithEmptySpeakers_ShouldThrowArgumentException()
    {
        // Arrange
        var startDateTime = DateTimeOffset.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Presentation.Create("Title", "Abstract", startDateTime, endDateTime, CreateTestRoom(), Array.Empty<Speaker>()));
        Assert.Contains("speaker", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithMultipleSpeakers_ShouldCreatePresentationWithMultipleSpeakers()
    {
        // Arrange
        var title = "Panel Discussion";
        var abstractText = "Discussion about technology";
        var startDateTime = DateTimeOffset.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);
        var room = CreateTestRoom();
        var speakers = new[] { CreateTestSpeaker(), CreateTestSpeaker(), CreateTestSpeaker() };

        // Act
        var presentation = Presentation.Create(title, abstractText, startDateTime, endDateTime, room, speakers);

        // Assert
        Assert.Equal(3, presentation.Speakers.Count);
    }

    [Fact]
    public void FromPersisted_WithValidData_ShouldCreatePresentation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Introduction to .NET";
        var abstractText = "Learn about .NET fundamentals";
        var startDateTime = DateTimeOffset.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);
        var room = CreateTestRoom();
        var speakers = new[] { CreateTestSpeaker() };

        // Act
        var presentation = Presentation.FromPersisted(id, title, abstractText, startDateTime, endDateTime, room, speakers, null);

        // Assert
        Assert.NotNull(presentation);
        Assert.Equal(id, presentation.Id);
        Assert.Equal(title, presentation.Title);
        Assert.Equal(DomainModelState.Pristine, presentation.State);
    }

    [Fact]
    public void FromPersisted_WithExternalId_ShouldCreatePresentationWithExternalId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Introduction to .NET";
        var abstractText = "Learn about .NET fundamentals";
        var startDateTime = DateTimeOffset.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);
        var room = CreateTestRoom();
        var speakers = new[] { CreateTestSpeaker() };
        var externalId = "ext-789";

        // Act
        var presentation = Presentation.FromPersisted(id, title, abstractText, startDateTime, endDateTime, room, speakers, externalId);

        // Assert
        Assert.Equal(externalId, presentation.ExternalId);
    }


}
