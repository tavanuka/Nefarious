using Nefarious.Common.Extensions;
using Nefarious.Core.Services;
using Nefarious.Spotify.Extensions;
using Nefarious.Spotify.Publishers;
using NetCord.Hosting.Gateway;
using Serilog;

// Two-stage initialization: https://github.com/serilog/serilog-aspnetcore?tab=readme-ov-file#two-stage-initialization
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);
    var configuration = builder.Configuration;
    var environment = builder.Environment;

    builder.Services
        .AddOptions(configuration)
        .AddLogging(configuration)
        .AddOpenTelemetry(configuration, environment);

    builder.AddRedisClient("nefarious-cache");
    builder.AddRedisDistributedCache(connectionName: "nefarious-cache");

    builder.Services
        .AddSpotifyClient()
        .AddDiscordGateway();
    
    builder.Services
        .AddHostedService<PlaylistEventDispatcher>()
        .AddHostedService<PlaylistMonitorPublisher>();

    var app = builder.Build();

    await app.RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}