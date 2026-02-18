using HexMaster.Attendr.IntegrationEvents.Events;

namespace HexMaster.Attendr.IntegrationEvents.Tests.Events;

/// <summary>
/// Concrete test implementation of the abstract IntegrationEvent.
/// </summary>
internal sealed class TestIntegrationEvent : IntegrationEvent
{
    public override string EventType => "test.event";
}

internal sealed class AnotherTestIntegrationEvent : IntegrationEvent
{
    public override string EventType => "another.test.event";
}

public class IntegrationEventTests
{
    [Fact]
    public void IntegrationEvent_EventId_IsGeneratedByDefault()
    {
        var evt = new TestIntegrationEvent();
        Assert.NotEqual(Guid.Empty, evt.EventId);
    }

    [Fact]
    public void IntegrationEvent_EventId_IsUniquePerInstance()
    {
        var evt1 = new TestIntegrationEvent();
        var evt2 = new TestIntegrationEvent();
        Assert.NotEqual(evt1.EventId, evt2.EventId);
    }

    [Fact]
    public void IntegrationEvent_OccurredAt_IsSetToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var evt = new TestIntegrationEvent();
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.InRange(evt.OccurredAt, before, after);
    }

    [Fact]
    public void IntegrationEvent_OccurredAt_IsUtcKind()
    {
        var evt = new TestIntegrationEvent();
        Assert.Equal(DateTimeKind.Utc, evt.OccurredAt.Kind);
    }

    [Fact]
    public void IntegrationEvent_EventId_CanBeOverriddenViaInit()
    {
        var expectedId = Guid.NewGuid();
        var evt = new TestIntegrationEvent { EventId = expectedId };
        Assert.Equal(expectedId, evt.EventId);
    }

    [Fact]
    public void IntegrationEvent_OccurredAt_CanBeOverriddenViaInit()
    {
        var expectedDate = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var evt = new TestIntegrationEvent { OccurredAt = expectedDate };
        Assert.Equal(expectedDate, evt.OccurredAt);
    }

    [Fact]
    public void IntegrationEvent_EventType_ReturnsCorrectValue()
    {
        var evt = new TestIntegrationEvent();
        Assert.Equal("test.event", evt.EventType);
    }

    [Fact]
    public void IntegrationEvent_DifferentSubclasses_HaveDifferentEventTypes()
    {
        var evt1 = new TestIntegrationEvent();
        var evt2 = new AnotherTestIntegrationEvent();
        Assert.NotEqual(evt1.EventType, evt2.EventType);
    }
}
