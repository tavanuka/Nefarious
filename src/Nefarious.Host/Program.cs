using Nefarious.Common.Options;
using Nefarious.Core.Extensions;
using Nefarious.Core.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);
    var config = builder.Configuration;

    builder.Services.AddSerilog();

    builder.Services.AddOptions<DiscordOptions>()
        .Bind(config.GetRequiredSection(DiscordOptions.SectionName))
        .ValidateOnStart();
    builder.Services.AddOptions<SpotifyOptions>()
        .Bind(config.GetRequiredSection(SpotifyOptions.SectionName))
        .ValidateOnStart();

    builder.Services.AddDiscordWebsocketClient(config);
    builder.Services.AddHostedService<NefariousBotService>();

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