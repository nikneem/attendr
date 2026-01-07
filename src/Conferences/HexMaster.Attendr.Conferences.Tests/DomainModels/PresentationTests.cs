using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Core.DomainModels;

namespace HexMaster.Attendr.Conferences.Tests.DomainModels;

public class PresentationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreatePresentation()
    {
        // Arrange
        var title = "Introduction to .NET";
        var abstractText = "Learn about .NET fundamentals";
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);
        var roomId = Guid.NewGuid();
        var speakerIds = new[] { Guid.NewGuid() };

        // Act
        var presentation = Presentation.Create(title, abstractText, startDateTime, endDateTime, roomId, speakerIds);

        // Assert
        Assert.NotNull(presentation);
        Assert.NotEqual(Guid.Empty, presentation.Id);
        Assert.Equal(title, presentation.Title);
        Assert.Equal(abstractText, presentation.Abstract);
        Assert.Equal(startDateTime, presentation.StartDateTime);
        Assert.Equal(endDateTime, presentation.EndDateTime);
        Assert.Equal(roomId, presentation.RoomId);
        Assert.Equal(speakerIds, presentation.SpeakerIds);
        Assert.Equal(DomainModelState.Created, presentation.State);
    }

    [Fact]
    public void Create_WithExternalId_ShouldCreatePresentationWithExternalId()
    {
        // Arrange
        var title = "Introduction to .NET";
        var abstractText = "Learn about .NET fundamentals";
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);
        var roomId = Guid.NewGuid();
        var speakerIds = new[] { Guid.NewGuid() };
        var externalId = "ext-789";

        // Act
        var presentation = Presentation.Create(title, abstractText, startDateTime, endDateTime, roomId, speakerIds, externalId);

        // Assert
        Assert.Equal(externalId, presentation.ExternalId);
    }

    [Fact]
    public void Create_WithNullTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Presentation.Create(null!, "Abstract", startDateTime, endDateTime, Guid.NewGuid(), new[] { Guid.NewGuid() }));
        Assert.Contains("title", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Presentation.Create(string.Empty, "Abstract", startDateTime, endDateTime, Guid.NewGuid(), new[] { Guid.NewGuid() }));
        Assert.Contains("title", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithNullAbstract_ShouldThrowArgumentException()
    {
        // Arrange
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Presentation.Create("Title", null!, startDateTime, endDateTime, Guid.NewGuid(), new[] { Guid.NewGuid() }));
        Assert.Contains("abstract", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithEmptyAbstract_ShouldThrowArgumentException()
    {
        // Arrange
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Presentation.Create("Title", string.Empty, startDateTime, endDateTime, Guid.NewGuid(), new[] { Guid.NewGuid() }));
        Assert.Contains("abstract", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_ShouldThrowArgumentException()
    {
        // Arrange
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(-1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Presentation.Create("Title", "Abstract", startDateTime, endDateTime, Guid.NewGuid(), new[] { Guid.NewGuid() }));
        Assert.Contains("end", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithEmptyRoomId_ShouldThrowArgumentException()
    {
        // Arrange
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Presentation.Create("Title", "Abstract", startDateTime, endDateTime, Guid.Empty, new[] { Guid.NewGuid() }));
        Assert.Contains("room", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithNullSpeakerIds_ShouldThrowArgumentNullException()
    {
        // Arrange
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            Presentation.Create("Title", "Abstract", startDateTime, endDateTime, Guid.NewGuid(), null!));
        Assert.Equal("speakerIds", exception.ParamName);
    }

    [Fact]
    public void Create_WithEmptySpeakerIds_ShouldThrowArgumentException()
    {
        // Arrange
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Presentation.Create("Title", "Abstract", startDateTime, endDateTime, Guid.NewGuid(), Array.Empty<Guid>()));
        Assert.Contains("speaker", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithMultipleSpeakers_ShouldCreatePresentationWithMultipleSpeakers()
    {
        // Arrange
        var title = "Panel Discussion";
        var abstractText = "Discussion about technology";
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);
        var roomId = Guid.NewGuid();
        var speakerIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var presentation = Presentation.Create(title, abstractText, startDateTime, endDateTime, roomId, speakerIds);

        // Assert
        Assert.Equal(3, presentation.SpeakerIds.Count());
    }

    [Fact]
    public void FromPersisted_WithValidData_ShouldCreatePresentation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Introduction to .NET";
        var abstractText = "Learn about .NET fundamentals";
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);
        var roomId = Guid.NewGuid();
        var speakerIds = new[] { Guid.NewGuid() };

        // Act
        var presentation = Presentation.FromPersisted(id, title, abstractText, startDateTime, endDateTime, roomId, speakerIds, null);

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
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);
        var roomId = Guid.NewGuid();
        var speakerIds = new[] { Guid.NewGuid() };
        var externalId = "ext-789";

        // Act
        var presentation = Presentation.FromPersisted(id, title, abstractText, startDateTime, endDateTime, roomId, speakerIds, externalId);

        // Assert
        Assert.Equal(externalId, presentation.ExternalId);
    }


}
