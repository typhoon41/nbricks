using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using GMForce.Bricks.Configuration;
using GMForce.Bricks.Persistence.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Filters;

namespace GMForce.Bricks.Logging;

internal class SerilogLogger
{
    private readonly string _logFilePath;
    private readonly LogEventLevel _defaultLogLevel;
    private readonly bool _isDevelopment;

    internal SerilogLogger(WebApplicationBuilder? builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        _isDevelopment = builder.Environment.IsDevelopment();
        _defaultLogLevel = _isDevelopment ? LogEventLevel.Debug : LogEventLevel.Warning;
        _logFilePath = builder.Configuration.ResolveFrom<string>("LogPath");
    }

    [SuppressMessage("Globalization", "CA1305:Specify IFormatProvider",
        Justification = "False positive. Requested specification like this in official documentation.")]
    internal ILogger Configure()
    {
        var baseConfig = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", _defaultLogLevel)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", _defaultLogLevel)
            .ReadFrom.Configuration(ConfigurationRetriever.Current);

        if (_isDevelopment)
        {
            _ = baseConfig.WriteTo.OpenTelemetry();
        }

        var logger = baseConfig.WriteTo.Logger(lc => DefaultFileConfiguration(lc, false)
                .Filter.ByExcluding(Matching.FromSource<AuditEventsHandler>()))
            .WriteTo.Logger(lc => DefaultFileConfiguration(lc, true)
                .Filter.ByIncludingOnly(Matching.FromSource<AuditEventsHandler>()))
            .CreateLogger();

        return logger;
    }

    internal void ConfigureBootstrap() =>
        Log.Logger = DefaultFileConfiguration(new LoggerConfiguration())
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .CreateBootstrapLogger();

    private string FilePath(bool isAudit)
    {
        if (!isAudit) return _logFilePath;
        var dir = Path.GetDirectoryName(_logFilePath) ?? string.Empty;
        var stem = Path.GetFileNameWithoutExtension(_logFilePath);
        return Path.Combine(dir, $"{stem}audit-.log");
    }

    private LoggerConfiguration DefaultFileConfiguration(LoggerConfiguration configuration, bool forAudit = false)
    {
        configuration.WriteTo.File(
                FilePath(forAudit),
                rollingInterval: RollingInterval.Day,
                shared: true,
                formatProvider: CultureInfo.InvariantCulture,
                outputTemplate: "[{Timestamp:HH:mm:ss.FFF} {Level}]: {Message:lj}{NewLine}{Exception}");

        return configuration;
    }
}
