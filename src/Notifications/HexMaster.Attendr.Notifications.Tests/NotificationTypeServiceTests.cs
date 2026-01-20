using HexMaster.Attendr.Notifications.Abstractions.Enums;
using HexMaster.Attendr.Notifications.Constants;
using HexMaster.Attendr.Notifications.Services;

namespace HexMaster.Attendr.Notifications.Tests;

public sealed class NotificationTypeServiceTests
{
    [Fact]
    public void GetTypeByKey_ReturnsKnownType()
    {
        var service = new NotificationTypeService();

        var result = service.GetTypeByKey(NotificationTypeKeys.ProfileCreated);

        Assert.NotNull(result);
        Assert.Equal(NotificationTypeKeys.ProfileCreated, result!.TypeKey);
        Assert.True(result.DefaultChannelSettings[NotificationChannel.InApp]);
    }

    [Fact]
    public void GetTypeByKey_WhenUnknown_ReturnsNull()
    {
        var service = new NotificationTypeService();

        var result = service.GetTypeByKey("unknown.type");

        Assert.Null(result);
        Assert.False(service.TypeExists("unknown.type"));
    }

    [Fact]
    public void GetAllTypes_ReturnsDistinctList()
    {
        var service = new NotificationTypeService();

        var allTypes = service.GetAllTypes();

        Assert.NotEmpty(allTypes);
        Assert.Equal(allTypes.Count, allTypes.Select(t => t.TypeKey).Distinct().Count());
    }
}
