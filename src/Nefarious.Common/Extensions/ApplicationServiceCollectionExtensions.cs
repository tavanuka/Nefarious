using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nefarious.Common.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

namespace Nefarious.Common.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers Options bindings from appsettings.json configuration file.
    /// </summary>
    /// <param name="services">The service collection context to use for registration.</param>
    /// <param name="configuration">Configuration context of the appsettings.json.</param>
    /// <returns>Updated service collection context.</returns>
    public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DiscordOptions>()
            .Bind(configuration.GetRequiredSection(DiscordOptions.SectionName))
            .ValidateOnStart();
        services.AddOptions<SpotifyOptions>()
            .Bind(configuration.GetRequiredSection(SpotifyOptions.SectionName))
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var otlpExporter = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        var serviceName = configuration["OTEL_SERVICE_NAME"] ?? "Undefined";

        services.AddSerilog((sp, loggerConfiguration) => {
            loggerConfiguration
                .ReadFrom.Configuration(configuration)
                .ReadFrom.Services(sp)
                .Enrich.FromLogContext()
                .WriteTo.Console();

            // This will get skipped if the env variable does not exist, making it optional.
            // In the future, it could be also conditionally turned on using a bool flag.
            if (!string.IsNullOrEmpty(otlpExporter))
                loggerConfiguration
                    .WriteTo.OpenTelemetry(options => {
                        options.Endpoint = otlpExporter;
                        options.ResourceAttributes.Add("service.name", serviceName);
                    });
        });

        return services;
    }

    public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var otlpExporter = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

        services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation())
            .WithTracing(tracing => {
                if (environment.IsDevelopment())
                    tracing.SetSampler(new AlwaysOnSampler());
            });

        // Configure telemetry providers if OTLP exporter exists.
        if (string.IsNullOrWhiteSpace(otlpExporter))
            return services;
        {
            services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
            services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
            services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());
        }

        return services;
    }
}