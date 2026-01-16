namespace HexMaster.Attendr.Aspire.AppHost;

public static class AspireConstants
{
    public const string ProfilesApiName = "hexmaster-attendr-profiles-api";
    public const string GroupsApiName = "hexmaster-attendr-groups-api";
    public const string ConferencesApiName = "hexmaster-attendr-conferences-api";
    public const string PresenceApiName = "hexmaster-attendr-presence-api";

    public static class TableStorage
    {
        public const string Profiles = "profiles";
        public const string Notifications = "notifications";
        public const string NotificationPreferences = "notificationpreferences";
    }

    public static class Dapr
    {
        public const string StateStoreName = "attendr-statestore";
        public const string PubSubName = "attendr-pubsub";
    }

    public static class Postgres
    {
        public const string Name = "attendr-postgress";
        public const string GroupsDatabase = "attendr-groups";
        public const string ConferencesDatabase = "attendr-conferences";
        public const string PresenceDatabase = "attendr-presence";
    }

}
