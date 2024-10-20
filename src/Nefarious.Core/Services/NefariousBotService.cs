using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nefarious.Common.Options;
using Nefarious.Common.Services.Discord;
using StackExchange.Redis;

namespace Nefarious.Core.Services;

public class NefariousBotService : DiscordWebsocketService<NefariousBotService>
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisChannel _channel;
    private IMessageChannel _guildChannel;
    public NefariousBotService(
        DiscordSocketClient client,
        ILogger<NefariousBotService> logger,
        IOptions<DiscordOptions> options,
        IConnectionMultiplexer redis)
        : base(client, logger, options)
    {
        _redis = redis;
        _channel = new RedisChannel("playlist_monitor", RedisChannel.PatternMode.Literal);
        _guildChannel = default!;
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        await _redis
            .GetSubscriber()
            .SubscribeAsync(_channel, (channel, message) => {
                _ = Task.Run(async () => {
                    Logger.LogInformation("Received playlist update notification from Redis: {Message}", message.ToString());
                    await NotifyChannel(message);
                }, token);
            });
        await base.ExecuteAsync(token);
    }

    protected override async Task OnClientReady()
    {
        if (await Client.GetChannelAsync(1297242079742922796) is IMessageChannel channel)
            _guildChannel = channel;
    }

    private async Task NotifyChannel(RedisValue message)
    {
        await _guildChannel.SendMessageAsync($"Status: {message}");
    }
}