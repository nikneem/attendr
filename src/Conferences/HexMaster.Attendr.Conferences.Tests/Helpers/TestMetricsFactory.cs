using System.Diagnostics.Metrics;
using Moq;

namespace HexMaster.Attendr.Conferences.Tests.Helpers;

/// <summary>
/// Factory for creating metrics instances for testing purposes.
/// </summary>
public static class TestMetricsFactory
{
    /// <summary>
    /// Creates a ConferenceMetrics instance with a mocked IMeterFactory for testing.
    /// </summary>
    public static Observability.ConferenceMetrics CreateConferenceMetrics()
    {
        var mockMeterFactory = new Mock<IMeterFactory>();
        var mockMeter = new Mock<Meter>("Test.Metrics", "1.0.0");

        mockMeterFactory
            .Setup(f => f.Create(It.IsAny<MeterOptions>()))
            .Returns(mockMeter.Object);

        return new Observability.ConferenceMetrics(mockMeterFactory.Object);
    }
}
