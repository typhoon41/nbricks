using System.Diagnostics.CodeAnalysis;
using GMForce.Bricks.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace GMForce.Bricks;

[ExcludeFromCodeCoverage]
public abstract class ApplicationRunner()
{
    protected WebApplicationBuilder? Builder { get; private set; }

    public void RunWith(WebApplicationOptions options)
    {
        try
        {
            Builder = WebApplication.CreateBuilder(options);
            new SerilogLogger(Builder).ConfigureBootstrap();
            Log.Information("Starting API host in {Environment} environment.", Builder.Environment.EnvironmentName);

            _ = Builder.WebHost.ConfigureKestrel(settings => settings.AddServerHeader = false);
            _ = Builder.Host.UseSerilog(new SerilogLogger(Builder).Configure());
            OnApplicationRun();
        }

        catch (Exception exception)
        {
            Log.Fatal(exception, "Host terminated unexpectedly");
            throw;
        }

        finally
        {
            Log.Information("Stopping web host");
            Log.CloseAndFlush();
        }
    }

    protected abstract void OnApplicationRun();
}
