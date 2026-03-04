using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

internal static class ConferencesApiResourceBuilderExtensions
{
    /// <summary>
    /// Adds a custom Aspire dashboard command that creates a dummy conference
    /// by invoking the CreateConferenceCommand directly via the development-only
    /// seed endpoint (/api/dev/conferences/seed), which requires no authorization.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithCreateDummyConferenceCommand(
        this IResourceBuilder<ProjectResource> builder)
    {
        builder.WithHttpCommand(
            path: "/api/dev/conferences/seed",
            displayName: "Create FutureTech Conference",
            endpointName: "http",
            commandOptions: new HttpCommandOptions
            {
                Description = "Creates a the future tech conference by invoking the development seed endpoint. This is useful for quickly seeding a conference for testing or demo purposes.",
                Method = HttpMethod.Post,
                IconName = "CalendarAdd",
                IsHighlighted = true
            });

        return builder;
    }
}
