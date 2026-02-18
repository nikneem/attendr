using HexMaster.Attendr.Core.DomainEvents;

namespace HexMaster.Attendr.Core.Tests.DomainEvents;

internal sealed record SampleEvent : DomainEvent
{
    public string Data { get; init; } = string.Empty;
}

public sealed class DomainEventTests
{
    [Fact]
    public void DomainEvent_HasUniqueIdByDefault()
    {
        var evt1 = new SampleEvent { Data = "a" };
        var evt2 = new SampleEvent { Data = "b" };
        Assert.NotEqual(evt1.Id, evt2.Id);
    }

    [Fact]
    public void DomainEvent_IdIsNotEmpty()
    {
        var evt = new SampleEvent();
        Assert.NotEqual(Guid.Empty, evt.Id);
    }

    [Fact]
    public void DomainEvent_OccurredAtIsSetToRecent()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var evt = new SampleEvent();
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        Assert.True(evt.OccurredAt >= before && evt.OccurredAt <= after);
    }

    [Fact]
    public void DomainEvent_CanOverrideId()
    {
        var customId = Guid.NewGuid();
        var evt = new SampleEvent { Id = customId };
        Assert.Equal(customId, evt.Id);
    }

    [Fact]
    public void DomainEvent_CanOverrideOccurredAt()
    {
        var customDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var evt = new SampleEvent { OccurredAt = customDate };
        Assert.Equal(customDate, evt.OccurredAt);
    }
}
