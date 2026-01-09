using Aspire.Hosting.Yarp.Transforms;
using HexMaster.Attendr.Aspire.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis").WithRedisInsight();
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(azurite =>
    {
        azurite.WithLifetime(ContainerLifetime.Persistent);
    });

var profilesTable = storage.AddTables(AspireConstants.TableStorage.Profiles);

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

var profilesApi = builder.AddProject<Projects.HexMaster_Attendr_Profiles_Api>(AspireConstants.ProfilesApiName)
    .WithDaprSidecar(opts =>
    {
        opts.WithReference(pubSub)
            .WithReference(stateStore);
    })
    .WaitFor(profilesTable)
    .WithReference(profilesTable);


// Add YARP gateway
var gateway = builder.AddYarp("gateway")
    .WithHostPort(5000)
    .WithConfiguration(yarp =>
    {
        // Proxy /profielen routes to the Profielen API
        yarp.AddRoute("/profiles/{**catch-all}", profilesApi)
            .WithTransformPathRemovePrefix("/profiles")
            .WithTransformPathPrefix("/api/profiles");

        //yarp.AddRoute("/users/{**catch-all}", usersApi)
        //    .WithTransformPathRemovePrefix("/users")
        //    .WithTransformPathPrefix("/api/users");

        //yarp.AddRoute("/memes/{**catch-all}", memesApi)
        //    .WithTransformPathRemovePrefix("/memes")
        //    .WithTransformPathPrefix("/api/memes");

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
