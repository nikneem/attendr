using HexMaster.Attendr.Core.DomainEvents;
using HexMaster.Attendr.Core.DomainModels;

namespace HexMaster.Attendr.Core.Tests.DomainModels;

// Concrete implementation of DomainModel<T> for tests
internal sealed class TestDomainEntity : DomainModel<Guid>
{
    private string _title = string.Empty;
    public string Title => _title;

    public TestDomainEntity() : base() { }

    public TestDomainEntity(Guid id) : base(id) { }

    public void CallSetId(Guid id) => SetId(id);
    public void CallSetCreatedOn(DateTimeOffset dt) => SetCreatedOn(dt);
    public void CallUpdateModifiedOn() => UpdateModifiedOn();
    public void CallSetModifiedOn(DateTimeOffset? dt) => SetModifiedOn(dt);
    public void CallAddDomainEvent(DomainEvent evt) => AddDomainEvent(evt);

    public void ChangeTitle(string title)
    {
        _title = title;
        UpdateModifiedOn();
    }
}

// Concrete DomainEvent for DomainModel tests
internal sealed record TestSimpleDomainEvent : DomainEvent
{
    public string Note { get; init; } = string.Empty;
}

public sealed class DomainModelTests
{
    // ──────────────────────────── Construction ────────────────────────────

    [Fact]
    public void DefaultConstructor_SetsIdToDefault()
    {
        var entity = new TestDomainEntity();
        Assert.Equal(default, entity.Id);
    }

    [Fact]
    public void Constructor_WithId_SetsId()
    {
        var id = Guid.NewGuid();
        var entity = new TestDomainEntity(id);

        Assert.Equal(id, entity.Id);
        Assert.True(entity.CreatedOn > DateTimeOffset.MinValue);
        Assert.Null(entity.ModifiedOn);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_SetsIdToEmpty()
    {
        // Guid.Empty is a valid value type value, not null
        var entity = new TestDomainEntity(Guid.Empty);
        Assert.Equal(Guid.Empty, entity.Id);
    }

    // ──────────────────────────── SetId ────────────────────────────

    [Fact]
    public void SetId_WithValidGuid_OverwritesId()
    {
        var entity = new TestDomainEntity();
        var id = Guid.NewGuid();
        entity.CallSetId(id);
        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void SetId_WithEmptyGuid_SetsId()
    {
        // Guid.Empty is a valid value type value, not null
        var entity = new TestDomainEntity();
        entity.CallSetId(Guid.Empty);
        Assert.Equal(Guid.Empty, entity.Id);
    }

    // ──────────────────────────── Date helpers ────────────────────────────

    [Fact]
    public void SetCreatedOn_SetsCreatedOn()
    {
        var entity = new TestDomainEntity();
        var dt = new DateTimeOffset(2023, 3, 15, 0, 0, 0, TimeSpan.Zero);
        entity.CallSetCreatedOn(dt);
        Assert.Equal(dt, entity.CreatedOn);
    }

    [Fact]
    public void UpdateModifiedOn_SetsModifiedOnToNow()
    {
        var entity = new TestDomainEntity(Guid.NewGuid());
        Assert.Null(entity.ModifiedOn);
        entity.CallUpdateModifiedOn();
        Assert.NotNull(entity.ModifiedOn);
        Assert.True(entity.ModifiedOn!.Value <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void SetModifiedOn_SetsExplicitDate()
    {
        var entity = new TestDomainEntity();
        var dt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        entity.CallSetModifiedOn(dt);
        Assert.Equal(dt, entity.ModifiedOn);
    }

    [Fact]
    public void SetModifiedOn_WithNull_SetsModifiedOnToNull()
    {
        var entity = new TestDomainEntity();
        entity.CallSetModifiedOn(null);
        Assert.Null(entity.ModifiedOn);
    }

    // ──────────────────────────── Domain events ────────────────────────────

    [Fact]
    public void DomainEvents_IsEmptyInitially()
    {
        var entity = new TestDomainEntity(Guid.NewGuid());
        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public void AddDomainEvent_AddsToCollection()
    {
        var entity = new TestDomainEntity(Guid.NewGuid());
        var evt = new TestSimpleDomainEvent { Note = "hello" };
        entity.CallAddDomainEvent(evt);
        Assert.Single(entity.DomainEvents);
    }

    [Fact]
    public void AddDomainEvent_WithNull_ThrowsArgumentNullException()
    {
        var entity = new TestDomainEntity(Guid.NewGuid());
        Assert.Throws<ArgumentNullException>(() => entity.CallAddDomainEvent(null!));
    }

    [Fact]
    public void ClearDomainEvents_RemovesAll()
    {
        var entity = new TestDomainEntity(Guid.NewGuid());
        entity.CallAddDomainEvent(new TestSimpleDomainEvent { Note = "a" });
        entity.CallAddDomainEvent(new TestSimpleDomainEvent { Note = "b" });
        entity.ClearDomainEvents();
        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public void ChangeTitle_UpdatesModifiedOn()
    {
        var entity = new TestDomainEntity(Guid.NewGuid());
        entity.ChangeTitle("New Title");
        Assert.Equal("New Title", entity.Title);
        Assert.NotNull(entity.ModifiedOn);
    }
}
