using HexMaster.Attendr.Conferences.DomainModels;
using HexMaster.Attendr.Core.DomainModels;

namespace HexMaster.Attendr.Conferences.Tests.DomainModels;

public class RoomTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateRoom()
    {
        // Arrange
        var name = "Main Hall";
        var capacity = 500;

        // Act
        var room = Room.Create(name, capacity);

        // Assert
        Assert.NotNull(room);
        Assert.NotEqual(Guid.Empty, room.Id);
        Assert.Equal(name, room.Name);
        Assert.Equal(capacity, room.Capacity);
        Assert.Equal(DomainModelState.Created, room.State);
    }

    [Fact]
    public void Create_WithExternalId_ShouldCreateRoomWithExternalId()
    {
        // Arrange
        var name = "Main Hall";
        var capacity = 500;
        var externalId = "ext-123";

        // Act
        var room = Room.Create(name, capacity, externalId);

        // Assert
        Assert.NotNull(room);
        Assert.Equal(externalId, room.ExternalId);
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Room.Create(null!, 100));
        Assert.Contains("name", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Room.Create(string.Empty, 100));
        Assert.Contains("name", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithZeroCapacity_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Room.Create("Room", 0));
        Assert.Contains("capacity", exception.Message.ToLower());
    }

    [Fact]
    public void Create_WithNegativeCapacity_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Room.Create("Room", -1));
        Assert.Contains("capacity", exception.Message.ToLower());
    }

    [Fact]
    public void FromPersisted_WithValidData_ShouldCreateRoom()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Main Hall";
        var capacity = 500;

        // Act
        var room = Room.FromPersisted(id, name, capacity, null);

        // Assert
        Assert.NotNull(room);
        Assert.Equal(id, room.Id);
        Assert.Equal(name, room.Name);
        Assert.Equal(capacity, room.Capacity);
        Assert.Equal(DomainModelState.Pristine, room.State);
    }

    [Fact]
    public void FromPersisted_WithExternalId_ShouldCreateRoomWithExternalId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Main Hall";
        var capacity = 500;
        var externalId = "ext-123";

        // Act
        var room = Room.FromPersisted(id, name, capacity, externalId);

        // Assert
        Assert.Equal(externalId, room.ExternalId);
    }


}
