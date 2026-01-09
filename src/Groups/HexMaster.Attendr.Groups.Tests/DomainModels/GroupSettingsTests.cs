using HexMaster.Attendr.Groups.DomainModels;
using HexMaster.Attendr.Groups.Abstractions.DomainModels;

namespace HexMaster.Attendr.Groups.Tests.DomainModels;

public sealed class GroupSettingsTests
{
    [Fact]
    public void Create_WithPublicAndSearchable_ShouldCreateSettings()
    {
        // Act
        var settings = GroupSettings.Create(isPublic: true, isSearchable: true);

        // Assert
        Assert.True(settings.IsPublic);
        Assert.True(settings.IsSearchable);
    }

    [Fact]
    public void Create_WithPrivateAndNotSearchable_ShouldCreateSettings()
    {
        // Act
        var settings = GroupSettings.Create(isPublic: false, isSearchable: false);

        // Assert
        Assert.False(settings.IsPublic);
        Assert.False(settings.IsSearchable);
    }

    [Fact]
    public void CreateDefault_ShouldCreatePrivateNonSearchableSettings()
    {
        // Act
        var settings = GroupSettings.CreateDefault();

        // Assert
        Assert.False(settings.IsPublic);
        Assert.False(settings.IsSearchable);
    }
}
