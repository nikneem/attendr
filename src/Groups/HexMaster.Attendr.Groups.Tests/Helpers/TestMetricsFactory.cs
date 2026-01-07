using System.Diagnostics.Metrics;
using HexMaster.Attendr.Groups.Observability;
using Moq;

namespace HexMaster.Attendr.Groups.Tests.Helpers;

public static class TestMetricsFactory
{
    public static GroupMetrics CreateGroupMetrics()
    {
        var mockMeterFactory = new Mock<IMeterFactory>();
        var meter = new Meter("HexMaster.Attendr.Groups.Tests");
        mockMeterFactory.Setup(x => x.Create(It.IsAny<MeterOptions>()))
            .Returns(meter);

        return new GroupMetrics(mockMeterFactory.Object);
    }
}
