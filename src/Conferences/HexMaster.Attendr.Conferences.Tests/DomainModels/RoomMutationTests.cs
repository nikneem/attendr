using HexMaster.Attendr.Conferences.DomainModels;

namespace HexMaster.Attendr.Conferences.Tests.DomainModels;

/// <summary>
/// Tests for Room mutation methods not covered in RoomTests.cs.
/// </summary>
public class RoomMutationTests
{
    // ── SetName ─────────────────────────────────────────────────────────────

    [Fact]
    public void SetName_WithNewName_ShouldUpdateName()
    {
        var room = Room.Create("Old Name", 100);

        room.SetName("New Name");

        Assert.Equal("New Name", room.Name);
    }

    [Fact]
    public void SetName_WithSameName_ShouldNotChangeProperty()
    {
        var room = Room.Create("Same Name", 100);

        room.SetName("Same Name");

        Assert.Equal("Same Name", room.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SetName_WithEmptyOrNull_ShouldThrowArgumentException(string? name)
    {
        var room = Room.Create("Room", 100);
        Assert.ThrowsAny<ArgumentException>(() => room.SetName(name!));
    }

    // ── SetCapacity ─────────────────────────────────────────────────────────

    [Fact]
    public void SetCapacity_WithValidCapacity_ShouldUpdateCapacity()
    {
        var room = Room.Create("Room", 50);

        room.SetCapacity(250);

        Assert.Equal(250, room.Capacity);
    }

    [Fact]
    public void SetCapacity_WithSameCapacity_ShouldNotChangeProperty()
    {
        var room = Room.Create("Room", 50);

        room.SetCapacity(50);

        Assert.Equal(50, room.Capacity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void SetCapacity_WithInvalidCapacity_ShouldThrowArgumentException(int capacity)
    {
        var room = Room.Create("Room", 100);
        Assert.Throws<ArgumentException>(() => room.SetCapacity(capacity));
    }
}
