// Two-stage initialization: https://github.com/serilog/serilog-aspnetcore?tab=readme-ov-file#two-stage-initialization
using Nefarious.Common.Extensions;
using Nefarious.Core.Extensions;
using Nefarious.Core.Services;
using Nefarious.Spotify.Extensions;
using Nefarious.Spotify.Publishers;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting application...");
try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;
    var environment = builder.Environment;
    
    // Add services to the container.
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    
    builder.Services
        .AddOptions(configuration)
        .AddLogging(configuration)
        .AddOpenTelemetry(configuration, environment);
    
    builder.AddRedisClient("nefarious-cache");
    builder.AddRedisDistributedCache(connectionName: "nefarious-cache");
    
    // Communication clients to various third party services go here.
    builder.Services.AddSpotifyClient();
    builder.Services.AddDiscordWebsocketClient(configuration);

    // Hosted services or background services go here.
    builder.Services.AddHostedService<NefariousBotService>();
    builder.Services.AddHostedService<PlaylistMonitorPublisher>();
    
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    
    app.UseAuthorization();
    app.MapControllers();
    
    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}