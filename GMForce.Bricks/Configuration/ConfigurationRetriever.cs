using Microsoft.Extensions.Configuration;

namespace GMForce.Bricks.Configuration;

internal static class ConfigurationRetriever
{
    internal static IConfiguration Current { get; } = GetCurrent();

    private static IConfiguration GetCurrent()
    {
        const string settingsFile = "appsettings";
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var environmentalConfiguration = $"{settingsFile}.{environment ?? "Test"}.json";

        // Needed due to Serilog file configuration.
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(settingsFile + ".json", false, true)
            .AddJsonFile(environmentalConfiguration, true)
            .Build();
    }
}
