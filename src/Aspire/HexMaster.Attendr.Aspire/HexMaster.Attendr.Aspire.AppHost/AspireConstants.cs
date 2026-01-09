using System;
using System.Collections.Generic;
using System.Text;

namespace HexMaster.Attendr.Aspire.AppHost;

public static class AspireConstants
{
    public const string ProfilesApiName = "hexmaster-attendr-profiles-api";
    public const string GroupsApiName = "hexmaster-attendr-groups-api";


    public static class TableStorage
    {
        public const string Profiles = "profiles";
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
    }

}
