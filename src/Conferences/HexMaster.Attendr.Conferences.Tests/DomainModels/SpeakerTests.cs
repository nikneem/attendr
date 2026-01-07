using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Core.DomainModels;

namespace HexMaster.Attendr.Conferences.Tests.DomainModels;

public class SpeakerTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateSpeaker()
    {
        // Arrange
        var name = "John Doe";

        // Act
        var speaker = Speaker.Create(name);

        // Assert
        Assert.NotNull(speaker);
        Assert.NotEqual(Guid.Empty, speaker.Id);
        Assert.Equal(name, speaker.Name);
        Assert.Equal(DomainModelState.Created, speaker.State);
    }

    [Fact]
    public void Create_WithCompany_ShouldCreateSpeakerWithCompany()
    {
        // Arrange
        var name = "John Doe";
        var company = "Acme Corp";

        // Act
        var speaker = Speaker.Create(name, company);

        // Assert
        Assert.Equal(company, speaker.Company);
    }

    [Fact]
    public void Create_WithProfilePictureUrl_ShouldCreateSpeakerWithProfilePicture()
    {
        // Arrange
        var name = "John Doe";
        var profilePictureUrl = "https://example.com/pic.jpg";

        // Act
        var speaker = Speaker.Create(name, profilePictureUrl: profilePictureUrl);

        // Assert
        Assert.Equal(profilePictureUrl, speaker.ProfilePictureUrl);
    }

    [Fact]
    public void Create_WithExternalId_ShouldCreateSpeakerWithExternalId()
    {
        // Arrange
        var name = "John Doe";
        var externalId = "ext-456";

        // Act
        var speaker = Speaker.Create(name, externalId: externalId);

        // Assert
        Assert.Equal(externalId, speaker.ExternalId);
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Speaker.Create(null!));
        Assert.Contains("name", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Speaker.Create(string.Empty));
        Assert.Contains("name", exception.Message.ToLower());
    }

    [Fact]
    public void FromPersisted_WithValidData_ShouldCreateSpeaker()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "John Doe";
        var company = "Acme Corp";
        var profilePictureUrl = "https://example.com/pic.jpg";

        // Act
        var speaker = Speaker.FromPersisted(id, name, company, profilePictureUrl, null);

        // Assert
        Assert.NotNull(speaker);
        Assert.Equal(id, speaker.Id);
        Assert.Equal(name, speaker.Name);
        Assert.Equal(company, speaker.Company);
        Assert.Equal(profilePictureUrl, speaker.ProfilePictureUrl);
        Assert.Equal(DomainModelState.Pristine, speaker.State);
    }

    [Fact]
    public void FromPersisted_WithExternalId_ShouldCreateSpeakerWithExternalId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "John Doe";
        var externalId = "ext-456";

        // Act
        var speaker = Speaker.FromPersisted(id, name, null, null, externalId);

        // Assert
        Assert.Equal(externalId, speaker.ExternalId);
    }


}
