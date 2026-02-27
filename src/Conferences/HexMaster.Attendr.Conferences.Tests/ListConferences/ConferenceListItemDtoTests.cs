using HexMaster.Attendr.Conferences.Features.ListConferences;

namespace HexMaster.Attendr.Conferences.Tests.ListConferences;

public class ConferenceListItemDtoTests
{
    private static ConferenceListItemDto CreateDto(Guid? id = null, string title = "Test Conf", bool isVisible = true) =>
        new ConferenceListItemDto(
            id ?? Guid.NewGuid(),
            title,
            "Amsterdam",
            "Netherlands",
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            DateOnly.FromDateTime(DateTime.Today.AddMonths(1).AddDays(3)),
            "https://example.com/image.jpg",
            isVisible,
            false,
            5,
            3,
            10);

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var id = Guid.NewGuid();
        var dto = CreateDto(id, "Test Conference", true);

        Assert.Equal(id, dto.Id);
        Assert.Equal("Test Conference", dto.Title);
        Assert.Equal("Amsterdam", dto.City);
        Assert.Equal("Netherlands", dto.Country);
        Assert.True(dto.IsVisible);
        Assert.Equal(5, dto.SpeakersCount);
        Assert.Equal(3, dto.RoomsCount);
        Assert.Equal(10, dto.PresentationsCount);
    }

    [Fact]
    public void TwoInstances_WithSameValues_AreEqual()
    {
        var id = Guid.NewGuid();
        var dto1 = CreateDto(id, "Same Title");
        var dto2 = CreateDto(id, "Same Title");

        Assert.Equal(dto1, dto2);
        Assert.True(dto1 == dto2);
    }

    [Fact]
    public void TwoInstances_WithDifferentTitles_AreNotEqual()
    {
        var id = Guid.NewGuid();
        var dto1 = CreateDto(id, "First");
        var dto2 = CreateDto(id, "Second");

        Assert.NotEqual(dto1, dto2);
        Assert.True(dto1 != dto2);
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHash()
    {
        var id = Guid.NewGuid();
        var dto1 = CreateDto(id, "Test");
        var dto2 = CreateDto(id, "Test");

        Assert.Equal(dto1.GetHashCode(), dto2.GetHashCode());
    }

    [Fact]
    public void ToString_ContainsTitle()
    {
        var dto = CreateDto(title: "TechConf2025");

        var str = dto.ToString();

        Assert.Contains("TechConf2025", str);
    }

    [Fact]
    public void WithExpression_ChangesTitle()
    {
        var dto = CreateDto(title: "Original");

        var updated = dto with { Title = "Updated" };

        Assert.Equal("Updated", updated.Title);
        Assert.Equal(dto.Id, updated.Id); // Other props unchanged
    }
}
