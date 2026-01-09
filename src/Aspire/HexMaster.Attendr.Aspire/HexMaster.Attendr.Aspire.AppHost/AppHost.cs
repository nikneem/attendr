using Azure.Provisioning.Storage;

var builder = DistributedApplication.CreateBuilder(args);


var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(azurite =>
    {
        azurite.WithLifetime(ContainerLifetime.Persistent);
    });

var profilesTable = storage.AddTables("profiles");

builder.AddProject<Projects.HexMaster_Attendr_Profiles_Api>("hexmaster-attendr-profiles-api")
    .WaitFor(profilesTable)
    .WithReference(profilesTable);

builder.Build().Run();
