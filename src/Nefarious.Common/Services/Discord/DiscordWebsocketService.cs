using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nefarious.Common.Options;

namespace Nefarious.Common.Services.Discord;

public abstract class DiscordWebsocketService<TBotService> : BackgroundService
{
    private const string LogTemplate = "[{Source}] {LogMessage}";
    protected readonly DiscordSocketClient Client;
    protected readonly DiscordOptions Options;
    protected readonly ILogger Logger;

    protected string ServiceName { get; }

    protected DiscordWebsocketService(DiscordSocketClient client, ILogger logger, IOptions<DiscordOptions> options)
    {
        Client = client;
        Logger = logger;
        Options = options.Value;
        ServiceName = GetServiceName();
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        Logger.LogInformation("Setting up discord bot");
        AddEventHandlers();

        try
        {
            await PreStartClient();
            await StartClient();
            await PostStartClient();
        }
        catch (Exception ex) when (ex is not TaskCanceledException)
        {
            await HandleStartupErrors(ex);
        }
    }

    protected virtual Task PreStartClient() => Task.CompletedTask;

    protected virtual Task PostStartClient() => Task.CompletedTask;

    protected async virtual Task StartClient()
    {
        await Client.LoginAsync(TokenType.Bot, Options.AccessTokens[ServiceName.ToLower()]);
        await Client.StartAsync();
    }

    protected async virtual Task StopClient()
    {
        await Client.LogoutAsync();
        await Client.StopAsync();
    }

    protected virtual void AddEventHandlers()
    {
        Client.Ready += OnClientReady;
        Client.Log += Log;
    }

    protected virtual void RemoveEventHandlers()
    {
        Client.Ready -= OnClientReady;
        Client.Log -= Log;
    }

    protected virtual Task OnClientReady()
    {
        return Task.CompletedTask;
    }

    protected async virtual Task HandleStartupErrors(Exception exception)
    {
        try
        {
            await Client.LogoutAsync();
        }
        finally
        {
            await Client.DisposeAsync();
        }
        Logger.LogError(exception, "An Exception was triggered on set-up");
    }

    protected virtual Task Log(LogMessage logMessage)
    {
        var severity = logMessage.Severity switch
        {
            LogSeverity.Debug => LogLevel.Debug,
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Warning => LogLevel.Warning,
            _ => LogLevel.Information
        };
        Logger.Log(logLevel: severity, exception: logMessage.Exception, LogTemplate, logMessage.Source, logMessage.Message);

        return Task.CompletedTask;
    }

    private static string GetServiceName() =>
        typeof(TBotService).Name is var objectName &&
        objectName.EndsWith("BotService")
            ? objectName.Replace("BotService", string.Empty)
            : throw new ArgumentException($"DiscordWebsocketService does not use correct service name: {objectName}. Please add suffix 'BotService'.");

    public override void Dispose()
    {
        RemoveEventHandlers();
        Client.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}