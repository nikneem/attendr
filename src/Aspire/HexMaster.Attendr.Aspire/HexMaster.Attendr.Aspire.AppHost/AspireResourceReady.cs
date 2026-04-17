namespace Aspire.Hosting;

internal static class ConferencesApiResourceBuilderExtensions
{

    /// <summary>
    /// Automatically seeds the database by calling the seed endpoint when the resource is ready.
    /// This ensures the database has initial data for testing and development purposes.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithAutoSeedOnReady(
        this IResourceBuilder<ProjectResource> builder)
    {
        builder.OnResourceReady(async (resource, evt, ct) =>
        {
            var endpoint = resource.GetEndpoint("http");
            var endpointUrl = endpoint.Url;

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(endpointUrl);

            try
            {
                var response = await httpClient.PostAsync("/api/dev/conferences/seed", null, ct);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the application startup
                Console.WriteLine($"Failed to seed conferences: {ex.Message}");
            }
        });

        return builder;
    }
}