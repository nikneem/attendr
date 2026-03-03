using Aspire.Hosting.Yarp.Transforms;
using HexMaster.Attendr.Aspire.AppHost;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Adding redis for caching and local pubsub
var redis = builder.AddRedis("redis").WithRedisInsight();

// Adding Azurite storage emulator for local development
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(azurite =>
    {
        azurite.WithLifetime(ContainerLifetime.Persistent);
    });

// ## Dapr Configuration - Adding a distributed state store with Redis as a storage backend ##
// ## Adding Pubsub with Redis as a broker ##
var redisHost = redis.Resource.PrimaryEndpoint.Property(EndpointProperty.Host);
var redisPort = redis.Resource.PrimaryEndpoint.Property(EndpointProperty.Port);
// # Dapr State Store and PubSub using Redis #
var stateStore = builder.AddDaprStateStore(AspireConstants.Dapr.StateStoreName)
    .WithMetadata(
        "redisHost",
        ReferenceExpression.Create($"{redisHost}:{redisPort}")
    )
    .WaitFor(redis);
var pubSub = builder
    .AddDaprPubSub(AspireConstants.Dapr.PubSubName)
    .WithMetadata(
        "redisHost",
        ReferenceExpression.Create($"{redisHost}:{redisPort}")
    )
    .WaitFor(redis);

// Adding PostgreSQL
// Add PostgreSQL for Memes service
var postgres = builder.AddPostgres(AspireConstants.Postgres.Name)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();


// ## The profiles service ##
var profilesTable = storage.AddTables(AspireConstants.TableStorage.Profiles);
var profileTopicsTable = storage.AddTables(AspireConstants.TableStorage.ProfileTopics);
var profilesApi = builder.AddProject<HexMaster_Attendr_Profiles_Api>(AspireConstants.ProfilesApiName)
    .WithDaprSidecar(opts =>
    {
        opts.WithReference(pubSub)
            .WithReference(stateStore);
    })
    .WithReference(profilesTable)
    .WithReference(profileTopicsTable)
    .WaitFor(profileTopicsTable)
    .WaitFor(profilesTable);

// ## The groups service ##
var groupsDatabase = postgres.AddDatabase(AspireConstants.Postgres.GroupsDatabase);
var groupsApi = builder.AddProject<HexMaster_Attendr_Groups_Api>(AspireConstants.GroupsApiName)
    .WithDaprSidecar(opts =>
    {
        opts.WithReference(pubSub)
            .WithReference(stateStore);
    })
    .WithReference(groupsDatabase)
    .WaitFor(groupsDatabase);

// ## The Conferences service ##
var conferencesDatabase = postgres.AddDatabase(AspireConstants.Postgres.ConferencesDatabase);
var conferencesApi = builder.AddProject<HexMaster_Attendr_Conferences_Api>(AspireConstants.ConferencesApiName)
    .WithDaprSidecar(opts =>
    {
        opts.WithReference(pubSub)
            .WithReference(stateStore);
    })
    .WithReference(conferencesDatabase)
    .WaitFor(conferencesDatabase);

// ## The Conferences service ##
var presenceDatabase = postgres.AddDatabase(AspireConstants.Postgres.PresenceDatabase);
var presenceApi = builder.AddProject<HexMaster_Attendr_Presence_Api>(AspireConstants.PresenceApiName)
    .WithDaprSidecar(opts =>
    {
        opts.WithReference(pubSub)
            .WithReference(stateStore);
    })
    .WithReference(presenceDatabase)
    .WaitFor(presenceDatabase);

// ## The Notifications service ##
var notificationPreferencesTable = storage.AddTables(AspireConstants.TableStorage.NotificationPreferences);
var notificationsTable = storage.AddTables(AspireConstants.TableStorage.Notifications);
var subscriptionsTable = storage.AddTables(AspireConstants.TableStorage.Subscriptions);
var notificationApi = builder.AddProject<HexMaster_Attendr_Notifications_Api>("hexmaster-attendr-notifications-api")
    .WithDaprSidecar(opts =>
    {
        opts.WithReference(pubSub)
            .WithReference(stateStore);
    })
    .WithReference(notificationPreferencesTable)
    .WithReference(notificationsTable)
    .WithReference(subscriptionsTable)
    .WaitFor(notificationPreferencesTable)
    .WaitFor(notificationsTable)
    .WaitFor(subscriptionsTable);

// Add YARP gateway
var gateway = builder.AddYarp("gateway")
    .WithHostPort(5000)
    .WithConfiguration(yarp =>
    {
        // Proxy /profielen routes to the Profielen API
        yarp.AddRoute("/profiles/{**catch-all}", profilesApi.GetEndpoint("http"))
            .WithTransformPathRemovePrefix("/profiles")
            .WithTransformPathPrefix("/api/profiles");

        yarp.AddRoute("/groups/{**catch-all}", groupsApi.GetEndpoint("http"))
            .WithTransformPathRemovePrefix("/groups")
            .WithTransformPathPrefix("/api/groups");

        yarp.AddRoute("/conferences/{**catch-all}", conferencesApi.GetEndpoint("http"))
            .WithTransformPathRemovePrefix("/conferences")
            .WithTransformPathPrefix("/api/conferences");

        yarp.AddRoute("/topics/{**catch-all}", conferencesApi.GetEndpoint("http"))
            .WithTransformPathRemovePrefix("/topics")
            .WithTransformPathPrefix("/api/topics");

        yarp.AddRoute("/presence/{**catch-all}", presenceApi.GetEndpoint("http"))
            .WithTransformPathRemovePrefix("/presence")
            .WithTransformPathPrefix("/api/presence");

        yarp.AddRoute("/notifications/{**catch-all}", notificationApi.GetEndpoint("http"))
            .WithTransformPathRemovePrefix("/notifications")
            .WithTransformPathPrefix("/api/notifications");

        // Route for SignalR hub - pass through without transformation
        // SignalR clients will connect to http://gateway:5000/hubs/games
        //yarp.AddRoute("/hubs/games/{**catch-all}", realtimeApi);
    });


var frontEndSourceFolder = Path.GetFullPath(builder.AppHostDirectory + "../../../../App");
if (Directory.Exists(frontEndSourceFolder))
{
    var frontend = builder.AddJavaScriptApp("frontend", frontEndSourceFolder)
        .WaitFor(gateway)
        .WithNpm(false)
        .WithRunScript("start")
        .WithHttpEndpoint(port: 4200, isProxied: false)
        .WithEnvironment("ASPIRE_GATEWAY_URL", gateway.GetEndpoint("http"));
}





builder.Build().Run();
