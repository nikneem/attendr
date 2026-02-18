using HexMaster.Attendr.Core.DomainModels;

namespace HexMaster.Attendr.Core.Tests.DomainModels;

public sealed class ExampleStatefulEntityTests
{
    // ──────────────────────────── Create ────────────────────────────

    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var id = Guid.NewGuid();
        var entity = ExampleStatefulEntity.Create(id, "Test Entity", "A description", 42);

        Assert.Equal(id, entity.Id);
        Assert.Equal("Test Entity", entity.Name);
        Assert.Equal("A description", entity.Description);
        Assert.Equal(42, entity.Value);
        Assert.Equal(DomainModelState.Created, entity.State);
    }

    [Fact]
    public void Create_WithoutOptionalParams_UsesDefaults()
    {
        var entity = ExampleStatefulEntity.Create(Guid.NewGuid(), "Minimal");
        Assert.Null(entity.Description);
        Assert.Equal(0, entity.Value);
        Assert.Equal(DomainModelState.Created, entity.State);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        Assert.Throws<ArgumentException>(() =>
            ExampleStatefulEntity.Create(Guid.NewGuid(), invalidName!));
    }

    // ──────────────────────────── Load ────────────────────────────

    [Fact]
    public void Load_WithValidData_SetsPropertiesAndPristineState()
    {
        var id = Guid.NewGuid();
        var createdOn = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var modifiedOn = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);

        var entity = ExampleStatefulEntity.Load(id, "Loaded", "desc", 10, createdOn, modifiedOn);

        Assert.Equal(id, entity.Id);
        Assert.Equal("Loaded", entity.Name);
        Assert.Equal("desc", entity.Description);
        Assert.Equal(10, entity.Value);
        Assert.Equal(DomainModelState.Pristine, entity.State);
        Assert.Equal(createdOn, entity.CreatedOn);
        Assert.Equal(modifiedOn, entity.ModifiedOn);
    }

    [Fact]
    public void Load_WithNullModifiedOn_SetsModifiedOnToNull()
    {
        var entity = ExampleStatefulEntity.Load(
            Guid.NewGuid(), "Name", null, 0,
            DateTimeOffset.UtcNow, null);

        Assert.Null(entity.ModifiedOn);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Load_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        Assert.Throws<ArgumentException>(() =>
            ExampleStatefulEntity.Load(Guid.NewGuid(), invalidName!, null, 0, DateTimeOffset.UtcNow, null));
    }

    // ──────────────────────────── UpdateName ────────────────────────────

    [Fact]
    public void UpdateName_WithNewName_SetsModified()
    {
        var entity = ExampleStatefulEntity.Load(
            Guid.NewGuid(), "OldName", null, 0, DateTimeOffset.UtcNow, null);

        entity.UpdateName("NewName");

        Assert.Equal("NewName", entity.Name);
        Assert.Equal(DomainModelState.Modified, entity.State);
        Assert.NotNull(entity.ModifiedOn);
    }

    [Fact]
    public void UpdateName_WithSameName_SetsTouched_NotModified()
    {
        var entity = ExampleStatefulEntity.Load(
            Guid.NewGuid(), "SameName", null, 0, DateTimeOffset.UtcNow, null);

        entity.UpdateName("SameName");

        Assert.Equal(DomainModelState.Touched, entity.State);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithInvalidName_ThrowsArgumentException(string? invalidName)
    {
        var entity = ExampleStatefulEntity.Load(
            Guid.NewGuid(), "Name", null, 0, DateTimeOffset.UtcNow, null);

        Assert.Throws<ArgumentException>(() => entity.UpdateName(invalidName!));
    }

    // ──────────────────────────── UpdateDescription ────────────────────────────

    [Fact]
    public void UpdateDescription_WithNewDescription_SetsModified()
    {
        var entity = ExampleStatefulEntity.Load(
            Guid.NewGuid(), "Name", null, 0, DateTimeOffset.UtcNow, null);

        entity.UpdateDescription("New Desc");

        Assert.Equal("New Desc", entity.Description);
        Assert.Equal(DomainModelState.Modified, entity.State);
    }

    [Fact]
    public void UpdateDescription_WithSameDescription_SetsTouched()
    {
        var entity = ExampleStatefulEntity.Load(
            Guid.NewGuid(), "Name", "Same", 0, DateTimeOffset.UtcNow, null);

        entity.UpdateDescription("Same");

        Assert.Equal(DomainModelState.Touched, entity.State);
    }

    [Fact]
    public void UpdateDescription_WithNull_SetsModifiedWhenPreviouslySet()
    {
        var entity = ExampleStatefulEntity.Load(
            Guid.NewGuid(), "Name", "OldDesc", 0, DateTimeOffset.UtcNow, null);

        entity.UpdateDescription(null);

        Assert.Null(entity.Description);
        Assert.Equal(DomainModelState.Modified, entity.State);
    }

    // ──────────────────────────── UpdateValue ────────────────────────────

    [Fact]
    public void UpdateValue_WithNewValue_SetsModified()
    {
        var entity = ExampleStatefulEntity.Load(
            Guid.NewGuid(), "Name", null, 5, DateTimeOffset.UtcNow, null);

        entity.UpdateValue(10);

        Assert.Equal(10, entity.Value);
        Assert.Equal(DomainModelState.Modified, entity.State);
        Assert.NotNull(entity.ModifiedOn);
    }

    [Fact]
    public void UpdateValue_WithSameValue_SetsTouched()
    {
        var entity = ExampleStatefulEntity.Load(
            Guid.NewGuid(), "Name", null, 5, DateTimeOffset.UtcNow, null);

        entity.UpdateValue(5);

        Assert.Equal(DomainModelState.Touched, entity.State);
    }

    // ──────────────────────────── MarkAsDeleted ────────────────────────────

    [Fact]
    public void MarkAsDeleted_FromPristine_SetsDeleted()
    {
        var entity = ExampleStatefulEntity.Load(
            Guid.NewGuid(), "Name", null, 0, DateTimeOffset.UtcNow, null);

        entity.MarkAsDeleted();

        Assert.Equal(DomainModelState.Deleted, entity.State);
    }

    [Fact]
    public void MarkAsDeleted_FromModified_SetsDeleted()
    {
        var entity = ExampleStatefulEntity.Load(
            Guid.NewGuid(), "Name", null, 0, DateTimeOffset.UtcNow, null);

        entity.UpdateName("Changed");
        entity.MarkAsDeleted();

        Assert.Equal(DomainModelState.Deleted, entity.State);
    }

    [Fact]
    public void MarkAsDeleted_FromCreated_DoesNotChangeState()
    {
        var entity = ExampleStatefulEntity.Create(Guid.NewGuid(), "Name");
        entity.MarkAsDeleted();
        // Created is terminal; SetState ignores the transition
        Assert.Equal(DomainModelState.Created, entity.State);
    }
}
