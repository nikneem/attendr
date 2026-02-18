using HexMaster.Attendr.IntegrationEvents.Models;

namespace HexMaster.Attendr.IntegrationEvents.Tests.Models;

public class NotificationTargetTests
{
    [Fact]
    public void NotificationTarget_Properties_CanBeSet()
    {
        var profileId = Guid.NewGuid();

        var target = new NotificationTarget
        {
            ProfileId = profileId,
            ProfileName = "John Doe"
        };

        Assert.Equal(profileId, target.ProfileId);
        Assert.Equal("John Doe", target.ProfileName);
    }

    [Fact]
    public void NotificationTarget_ProfileId_CanBeAnyGuid()
    {
        var id = Guid.NewGuid();
        var target = new NotificationTarget { ProfileId = id, ProfileName = "Test" };
        Assert.Equal(id, target.ProfileId);
    }

    [Fact]
    public void NotificationTarget_ProfileName_IsStored()
    {
        const string name = "Alice";
        var target = new NotificationTarget { ProfileId = Guid.NewGuid(), ProfileName = name };
        Assert.Equal(name, target.ProfileName);
    }

    [Fact]
    public void MultipleNotificationTargets_CanBeCreated()
    {
        var targets = new List<NotificationTarget>
        {
            new() { ProfileId = Guid.NewGuid(), ProfileName = "Admin A" },
            new() { ProfileId = Guid.NewGuid(), ProfileName = "Admin B" }
        };

        Assert.Equal(2, targets.Count);
        Assert.All(targets, t => Assert.NotEqual(Guid.Empty, t.ProfileId));
    }
}
