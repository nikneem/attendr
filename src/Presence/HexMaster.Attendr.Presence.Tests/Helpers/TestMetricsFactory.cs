using System.Diagnostics.Metrics;
using HexMaster.Attendr.Presence.Observability;

namespace HexMaster.Attendr.Presence.Tests.Helpers;

/// <summary>
/// Factory for creating test instances of metrics.
/// </summary>
public static class TestMetricsFactory
{
    public static PresenceMetrics CreatePresenceMetrics()
    {
        var meterFactory = new TestMeterFactory();
        return new PresenceMetrics(meterFactory);
    }

    private class TestMeterFactory : IMeterFactory
    {
        public Meter Create(MeterOptions options)
        {
            return new Meter(options);
        }

        public void Dispose()
        {
        }
    }
}
