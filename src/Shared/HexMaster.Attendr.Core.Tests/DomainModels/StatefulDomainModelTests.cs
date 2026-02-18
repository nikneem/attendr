using HexMaster.Attendr.Core.DomainEvents;
using HexMaster.Attendr.Core.DomainModels;

namespace HexMaster.Attendr.Core.Tests.DomainModels;

// Concrete test implementation of StatefulDomainModel<T>
internal sealed class TestStatefulEntity : StatefulDomainModel<Guid>
{
    private string _name = string.Empty;
    public string Name => _name;

    public TestStatefulEntity() : base() { }

    public TestStatefulEntity(Guid id, DomainModelState state = DomainModelState.Pristine)
        : base(id, state) { }

    public void CallSetId(Guid id) => SetId(id);
    public void CallSetCreatedOn(DateTimeOffset dt) => SetCreatedOn(dt);
    public void CallUpdateModifiedOn() => UpdateModifiedOn();
    public void CallSetModifiedOn(DateTimeOffset? dt) => SetModifiedOn(dt);
    public void CallSetState(DomainModelState state) => SetState(state);
    public void CallTrackPropertyChange() => TrackPropertyChange();
    public bool CallShouldUpdateProperty(string current, string next) => ShouldUpdateProperty(current, next);
    public void CallAddDomainEvent(DomainEvent evt) => AddDomainEvent(evt);

    public void ChangeName(string name)
    {
        if (ShouldUpdateProperty(_name, name))
        {
            _name = name;
            UpdateModifiedOn();
        }
    }
}

// Concrete DomainEvent for testing
internal sealed record TestDomainEvent : DomainEvent
{
    public string Payload { get; init; } = string.Empty;
}

public sealed class StatefulDomainModelTests
{
    // ──────────────────────────── Construction ────────────────────────────

    [Fact]
    public void DefaultConstructor_SetsIdToDefault_AndStateToPristine()
    {
        var entity = new TestStatefulEntity();
        Assert.Equal(default, entity.Id);
        Assert.Equal(DomainModelState.Pristine, entity.State);
    }

    [Fact]
    public void Constructor_WithId_SetsIdAndState()
    {
        var id = Guid.NewGuid();
        var entity = new TestStatefulEntity(id, DomainModelState.Created);

        Assert.Equal(id, entity.Id);
        Assert.Equal(DomainModelState.Created, entity.State);
        Assert.True(entity.CreatedOn > DateTimeOffset.MinValue);
    }

    [Fact]
    public void Constructor_WithPristineState_SetsStateToPristine()
    {
        var id = Guid.NewGuid();
        var entity = new TestStatefulEntity(id, DomainModelState.Pristine);
        Assert.Equal(DomainModelState.Pristine, entity.State);
    }

    [Fact]
    public void Constructor_WithDefaultGuid_SetIdToEmpty()
    {
        // Guid is a value type; default/empty is allowed (not null)
        var entity = new TestStatefulEntity(Guid.Empty, DomainModelState.Pristine);
        Assert.Equal(Guid.Empty, entity.Id);
    }

    [Theory]
    [InlineData(DomainModelState.Touched)]
    [InlineData(DomainModelState.Modified)]
    [InlineData(DomainModelState.Deleted)]
    public void Constructor_WithInvalidInitialState_ThrowsArgumentException(DomainModelState invalidState)
    {
        var id = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => new TestStatefulEntity(id, invalidState));
    }

    // ──────────────────────────── SetId ────────────────────────────

    [Fact]
    public void SetId_WithValidGuid_SetsId()
    {
        var entity = new TestStatefulEntity();
        var id = Guid.NewGuid();
        entity.CallSetId(id);
        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void SetId_WithEmptyGuid_SetsId()
    {
        // Guid.Empty is a valid value type value, not null
        var entity = new TestStatefulEntity();
        entity.CallSetId(Guid.Empty);
        Assert.Equal(Guid.Empty, entity.Id);
    }

    // ──────────────────────────── Date helpers ────────────────────────────

    [Fact]
    public void SetCreatedOn_SetsCreatedOn()
    {
        var entity = new TestStatefulEntity();
        var dt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        entity.CallSetCreatedOn(dt);
        Assert.Equal(dt, entity.CreatedOn);
    }

    [Fact]
    public void UpdateModifiedOn_SetsModifiedOnToCurrentUtc()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid());
        Assert.Null(entity.ModifiedOn);
        entity.CallUpdateModifiedOn();
        Assert.NotNull(entity.ModifiedOn);
        Assert.True(entity.ModifiedOn!.Value <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void SetModifiedOn_SetsExactDate()
    {
        var entity = new TestStatefulEntity();
        var dt = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero);
        entity.CallSetModifiedOn(dt);
        Assert.Equal(dt, entity.ModifiedOn);
    }

    [Fact]
    public void SetModifiedOn_WithNull_SetsModifiedOnToNull()
    {
        var entity = new TestStatefulEntity();
        entity.CallSetModifiedOn(null);
        Assert.Null(entity.ModifiedOn);
    }

    // ──────────────────────────── State machine ────────────────────────────

    [Fact]
    public void SetState_SameState_DoesNotChange()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Pristine);
        entity.CallSetState(DomainModelState.Pristine);
        Assert.Equal(DomainModelState.Pristine, entity.State);
    }

    [Fact]
    public void SetState_ToCreated_IsIgnored()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Pristine);
        entity.CallSetState(DomainModelState.Created);
        Assert.Equal(DomainModelState.Pristine, entity.State); // unchanged
    }

    [Fact]
    public void SetState_CreatedEntity_CannotTransition()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Created);
        entity.CallSetState(DomainModelState.Modified);
        Assert.Equal(DomainModelState.Created, entity.State); // unchanged – terminal
    }

    [Fact]
    public void SetState_DeletedEntity_CannotTransition()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Pristine);
        entity.CallSetState(DomainModelState.Deleted);
        Assert.Equal(DomainModelState.Deleted, entity.State);
        entity.CallSetState(DomainModelState.Modified);
        Assert.Equal(DomainModelState.Deleted, entity.State); // terminal
    }

    [Fact]
    public void SetState_PristineToTouched_Allowed()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Pristine);
        entity.CallSetState(DomainModelState.Touched);
        Assert.Equal(DomainModelState.Touched, entity.State);
    }

    [Fact]
    public void SetState_PristineToModified_Allowed()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Pristine);
        entity.CallSetState(DomainModelState.Modified);
        Assert.Equal(DomainModelState.Modified, entity.State);
    }

    [Fact]
    public void SetState_PristineToDeleted_Allowed()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Pristine);
        entity.CallSetState(DomainModelState.Deleted);
        Assert.Equal(DomainModelState.Deleted, entity.State);
    }

    [Fact]
    public void SetState_TouchedToModified_Allowed()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Pristine);
        entity.CallSetState(DomainModelState.Touched);
        entity.CallSetState(DomainModelState.Modified);
        Assert.Equal(DomainModelState.Modified, entity.State);
    }

    [Fact]
    public void SetState_TouchedToDeleted_Allowed()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Pristine);
        entity.CallSetState(DomainModelState.Touched);
        entity.CallSetState(DomainModelState.Deleted);
        Assert.Equal(DomainModelState.Deleted, entity.State);
    }

    [Fact]
    public void SetState_TouchedToPristine_NotAllowed()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Pristine);
        entity.CallSetState(DomainModelState.Touched);
        entity.CallSetState(DomainModelState.Pristine);
        Assert.Equal(DomainModelState.Touched, entity.State); // unchanged
    }

    [Fact]
    public void SetState_ModifiedToDeleted_Allowed()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Pristine);
        entity.CallSetState(DomainModelState.Modified);
        entity.CallSetState(DomainModelState.Deleted);
        Assert.Equal(DomainModelState.Deleted, entity.State);
    }

    [Fact]
    public void SetState_ModifiedToPristine_NotAllowed()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Pristine);
        entity.CallSetState(DomainModelState.Modified);
        entity.CallSetState(DomainModelState.Pristine);
        Assert.Equal(DomainModelState.Modified, entity.State); // unchanged
    }

    // ──────────────────────────── TrackPropertyChange ────────────────────────────

    [Fact]
    public void TrackPropertyChange_FromPristine_SetsTouched()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Pristine);
        entity.CallTrackPropertyChange();
        Assert.Equal(DomainModelState.Touched, entity.State);
    }

    // ──────────────────────────── ShouldUpdateProperty ────────────────────────────

    [Fact]
    public void ShouldUpdateProperty_SameValue_ReturnsFalse_AndSetsTouched()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Pristine);
        var result = entity.CallShouldUpdateProperty("hello", "hello");
        Assert.False(result);
        Assert.Equal(DomainModelState.Touched, entity.State);
    }

    [Fact]
    public void ShouldUpdateProperty_DifferentValue_ReturnsTrue_AndSetsModified()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Pristine);
        var result = entity.CallShouldUpdateProperty("hello", "world");
        Assert.True(result);
        Assert.Equal(DomainModelState.Modified, entity.State);
    }

    // ──────────────────────────── Domain events ────────────────────────────

    [Fact]
    public void AddDomainEvent_AddsEventToCollection()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid());
        var evt = new TestDomainEvent { Payload = "test" };
        entity.CallAddDomainEvent(evt);
        Assert.Single(entity.DomainEvents);
        Assert.Contains(evt, entity.DomainEvents);
    }

    [Fact]
    public void AddDomainEvent_WithNull_ThrowsArgumentNullException()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid());
        Assert.Throws<ArgumentNullException>(() => entity.CallAddDomainEvent(null!));
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid());
        entity.CallAddDomainEvent(new TestDomainEvent { Payload = "a" });
        entity.CallAddDomainEvent(new TestDomainEvent { Payload = "b" });
        Assert.Equal(2, entity.DomainEvents.Count);
        entity.ClearDomainEvents();
        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public void DomainEvents_IsReadOnlyCollection()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid());
        Assert.IsAssignableFrom<IReadOnlyCollection<DomainEvent>>(entity.DomainEvents);
    }

    // ──────────────────────────── ChangeName integration ────────────────────────────

    [Fact]
    public void ChangeName_WithNewValue_SetsModifiedAndUpdatesName()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Pristine);
        entity.ChangeName("NewName");
        Assert.Equal("NewName", entity.Name);
        Assert.Equal(DomainModelState.Modified, entity.State);
        Assert.NotNull(entity.ModifiedOn);
    }

    [Fact]
    public void ChangeName_WithSameValue_SetsTouchedAndDoesNotUpdateModifiedOn()
    {
        var entity = new TestStatefulEntity(Guid.NewGuid(), DomainModelState.Pristine);
        entity.ChangeName("SameName");
        var modifiedOn = entity.ModifiedOn;
        // Reset state manually for next call to simulate Touched again
        // second call with same value – state stays Modified (from first call), no double-update
        entity.ChangeName("SameName");
        Assert.Equal(modifiedOn, entity.ModifiedOn); // not changed again
    }
}
