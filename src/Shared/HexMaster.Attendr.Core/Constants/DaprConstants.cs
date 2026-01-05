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
        /// The default pub/sub component name used for integration events.
        /// </summary>
        public const string DefaultPubSubName = "pubsub";

        /// <summary>
        /// The Dapr pub/sub component name.
        /// </summary>
        public const string DaprPubSubName = "dapr-pubsub";
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

    /// <summary>
    /// Event topic names for integration events.
    /// </summary>
    public static class Topics
    {
        /// <summary>
        /// Topic for conference created events.
        /// </summary>
        public const string ConferenceCreated = "conference.created";

        /// <summary>
        /// Topic for conference updated events.
        /// </summary>
        public const string ConferenceUpdated = "conference.updated";

        /// <summary>
        /// Topic for profile created events.
        /// </summary>
        public const string ProfileCreated = "profile.created";

        /// <summary>
        /// Topic for profile updated events.
        /// </summary>
        public const string ProfileUpdated = "profile.updated";

        /// <summary>
        /// Topic for profile followed conference events.
        /// </summary>
        public const string ProfileFollowedConference = "profile-followed-conference";

        /// <summary>
        /// Topic for profiles followed conference events (bulk).
        /// </summary>
        public const string ProfilesFollowedConference = "profiles-followed-conference";
    }
}
