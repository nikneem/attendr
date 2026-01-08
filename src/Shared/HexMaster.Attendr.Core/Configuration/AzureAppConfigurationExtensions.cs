using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace HexMaster.Attendr.Core.Configuration;

/// <summary>
/// Extension methods for configuring Azure App Configuration.
/// </summary>
public static class AzureAppConfigurationExtensions
{
    /// <summary>
    /// Adds Azure App Configuration to the configuration builder when running in Release mode.
    /// Uses managed identity for authentication and supports Key Vault references.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <param name="environment">The environment name.</param>
    /// <returns>The configuration builder.</returns>
    public static IConfigurationBuilder AddAttendrAzureAppConfiguration(
        this IConfigurationBuilder builder,
        string environment)
    {
#if RELEASE
        var tempConfig = builder.Build();
        var appConfigEndpoint = tempConfig["AppConfiguration:Endpoint"] 
                             ?? tempConfig["AppConfiguration__Endpoint"]
                             ?? Environment.GetEnvironmentVariable("AppConfiguration__Endpoint");
        Console.WriteLine($"Azure App Configuration Endpoint: {appConfigEndpoint}");
        if (!string.IsNullOrWhiteSpace(appConfigEndpoint))
        {
            try{
                builder.AddAzureAppConfiguration(options =>
                {
                    var credential = new DefaultAzureCredential();
                    
                    options.Connect(new Uri(appConfigEndpoint), credential)
                        .Select("*", environment)
                        .ConfigureKeyVault(kv =>
                        {
                            kv.SetCredential(credential);
                        });
                    
                    Console.WriteLine($"Azure App Configuration - Loading keys with label: '{environment}'");
                });
                Console.WriteLine($"Configuration from {appConfigEndpoint} succeeded.");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error connecting to Azure App Configuration: {ex.Message}");
                throw;
            }
        }
#endif
        return builder;
    }
}