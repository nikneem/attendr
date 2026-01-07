using System.Diagnostics.Metrics;
using HexMaster.Attendr.Profiles.Observability;
using Moq;

namespace HexMaster.Attendr.Profiles.Tests.Helpers;

/// <summary>
/// Factory for creating metrics instances for testing purposes.
/// </summary>
public static class TestMetricsFactory
{
    /// <summary>
    /// Creates a ProfileMetrics instance with a mocked IMeterFactory for testing.
    /// </summary>
    /// <returns>A ProfileMetrics instance suitable for unit testing.</returns>
    public static ProfileMetrics CreateProfileMetrics()
    {
        var mockMeterFactory = new Mock<IMeterFactory>();
        var mockMeter = new Mock<Meter>("Test.Metrics", "1.0.0");

        mockMeterFactory
            .Setup(f => f.Create(It.IsAny<MeterOptions>()))
            .Returns(mockMeter.Object);

        return new ProfileMetrics(mockMeterFactory.Object);
    }
}
