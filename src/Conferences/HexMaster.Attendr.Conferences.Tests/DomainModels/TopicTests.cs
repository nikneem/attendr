using HexMaster.Attendr.Conferences.DomainModels;

namespace HexMaster.Attendr.Conferences.Tests.DomainModels;

public class TopicTests
{
    // ── Create ─────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidKeyAndName_ShouldReturnHiddenTopic()
    {
        var topic = Topic.Create("dotnet", ".NET");

        Assert.NotNull(topic);
        Assert.NotEqual(Guid.Empty, topic.Id);
        Assert.Equal("dotnet", topic.Key);
        Assert.Equal(".NET", topic.Name);
        Assert.False(topic.IsVisible);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrWhitespaceKey_ShouldThrowArgumentException(string? key)
    {
        Assert.Throws<ArgumentException>(() => Topic.Create(key!, "Name"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrWhitespaceName_ShouldThrowArgumentException(string? name)
    {
        Assert.Throws<ArgumentException>(() => Topic.Create("key", name!));
    }

    // ── CreateManually ─────────────────────────────────────────────────────

    [Fact]
    public void CreateManually_WithValidKeyAndName_ShouldReturnVisibleTopic()
    {
        var topic = Topic.CreateManually("azure", "Azure");

        Assert.NotNull(topic);
        Assert.NotEqual(Guid.Empty, topic.Id);
        Assert.Equal("azure", topic.Key);
        Assert.Equal("Azure", topic.Name);
        Assert.True(topic.IsVisible);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateManually_WithEmptyKey_ShouldThrowArgumentException(string? key)
    {
        Assert.Throws<ArgumentException>(() => Topic.CreateManually(key!, "Name"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateManually_WithEmptyName_ShouldThrowArgumentException(string? name)
    {
        Assert.Throws<ArgumentException>(() => Topic.CreateManually("key", name!));
    }

    // ── FromPersisted ──────────────────────────────────────────────────────

    [Fact]
    public void FromPersisted_WithValidData_ShouldRestoreTopicState()
    {
        var id = Guid.NewGuid();
        var createdOn = DateTimeOffset.UtcNow.AddDays(-10);

        var topic = Topic.FromPersisted(id, "cloud", "Cloud", isVisible: true, createdOn);

        Assert.Equal(id, topic.Id);
        Assert.Equal("cloud", topic.Key);
        Assert.Equal("Cloud", topic.Name);
        Assert.True(topic.IsVisible);
    }

    [Fact]
    public void FromPersisted_WithInvisibleFlag_ShouldBeHidden()
    {
        var topic = Topic.FromPersisted(Guid.NewGuid(), "k", "n", isVisible: false, DateTimeOffset.UtcNow);
        Assert.False(topic.IsVisible);
    }

    // ── MakeVisible ────────────────────────────────────────────────────────

    [Fact]
    public void MakeVisible_WhenHidden_ShouldSetIsVisibleTrue()
    {
        var topic = Topic.Create("key", "Name"); // created hidden
        Assert.False(topic.IsVisible);

        topic.MakeVisible();

        Assert.True(topic.IsVisible);
    }

    [Fact]
    public void MakeVisible_WhenAlreadyVisible_ShouldRemainVisible()
    {
        var topic = Topic.CreateManually("key", "Name");
        topic.MakeVisible();

        Assert.True(topic.IsVisible);
    }

    // ── Hide ───────────────────────────────────────────────────────────────

    [Fact]
    public void Hide_WhenVisible_ShouldSetIsVisibleFalse()
    {
        var topic = Topic.CreateManually("key", "Name");
        Assert.True(topic.IsVisible);

        topic.Hide();

        Assert.False(topic.IsVisible);
    }

    [Fact]
    public void Hide_WhenAlreadyHidden_ShouldRemainHidden()
    {
        var topic = Topic.Create("key", "Name");
        topic.Hide();

        Assert.False(topic.IsVisible);
    }

    // ── UpdateDetails ───────────────────────────────────────────────────────

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateKeyAndName()
    {
        var topic = Topic.Create("oldKey", "Old Name");

        topic.UpdateDetails("newKey", "New Name");

        Assert.Equal("newKey", topic.Key);
        Assert.Equal("New Name", topic.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateDetails_WithEmptyKey_ShouldThrowArgumentException(string? key)
    {
        var topic = Topic.Create("key", "Name");
        Assert.Throws<ArgumentException>(() => topic.UpdateDetails(key!, "Name"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateDetails_WithEmptyName_ShouldThrowArgumentException(string? name)
    {
        var topic = Topic.Create("key", "Name");
        Assert.Throws<ArgumentException>(() => topic.UpdateDetails("key", name!));
    }
}
