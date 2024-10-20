using Nefarious.Common.Options;
using Nefarious.Core.Extensions;
using Nefarious.Core.Services;
using Nefarious.Spotify.Extensions;
using Nefarious.Spotify.Publishers;
using Nefarious.Spotify.Services;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

// Two-stage initialization: https://github.com/serilog/serilog-aspnetcore?tab=readme-ov-file#two-stage-initialization
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);
    var config = builder.Configuration;

    var otlpExporter = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
    var serviceName = builder.Configuration["OTEL_SERVICE_NAME"] ?? "Undefined";

    builder.Services.AddOptions<DiscordOptions>()
        .Bind(config.GetRequiredSection(DiscordOptions.SectionName))
        .ValidateOnStart();
    builder.Services.AddOptions<SpotifyOptions>()
        .Bind(config.GetRequiredSection(SpotifyOptions.SectionName))
        .ValidateOnStart();

    builder.Services.AddSerilog((sp, loggerConfiguration) => {
        loggerConfiguration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext()
            .WriteTo.Console();

        if (!string.IsNullOrEmpty(otlpExporter))
            loggerConfiguration
                .WriteTo.OpenTelemetry(options => {
                    options.Endpoint = otlpExporter;
                    options.ResourceAttributes.Add("service.name", serviceName);
                });
    });

    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics => metrics
            .AddRuntimeInstrumentation())
        .WithTracing(tracing => {
            if (builder.Environment.IsDevelopment())
                tracing.SetSampler(new AlwaysOnSampler());
        });

    // Add OTLP Exporters.
    if (!string.IsNullOrWhiteSpace(otlpExporter))
    {
        builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
        builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
        builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());
    }

    builder.AddRedisClient("nefarious-cache");
    builder.AddRedisDistributedCache(connectionName: "nefarious-cache");
    
    builder.Services.AddSpotifyClient();
    builder.Services.AddDiscordWebsocketClient(config);
    
    builder.Services.AddSingleton<IPlaylistService, PlaylistService>();
    
    builder.Services.AddHostedService<NefariousBotService>();
    builder.Services.AddHostedService<PlaylistMonitorPublisher>();

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