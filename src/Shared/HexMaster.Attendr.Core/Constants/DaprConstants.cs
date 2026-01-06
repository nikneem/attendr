namespace HexMaster.Attendr.Core.Constants;

/// <summary>
/// Contains constants for Dapr component names used across the Attendr application.
/// </summary>
public static class DaprConstants
{
    /// <summary>
    /// Dapr component names for pub/sub.
    /// </summary>
    public static class PubSub
    {
        /// <summary>
        /// The pub/sub component name used for integration events.
        /// </summary>
        public const string Name = "pubsub";
    }

    /// <summary>
    /// Dapr component names for state stores.
    /// </summary>
    public static class StateStore
    {
        /// <summary>
        /// The shared state store component name.
        /// </summary>
        public const string SharedStateStoreName = "statestore";
    }
}
