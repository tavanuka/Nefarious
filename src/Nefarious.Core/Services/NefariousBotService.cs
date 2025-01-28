using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nefarious.Common.Events;
using Nefarious.Common.Options;
using Nefarious.Common.Services.Discord;
using StackExchange.Redis;
using System.Text.Json;

namespace Nefarious.Core.Services;

public class NefariousBotService : DiscordWebsocketService<NefariousBotService>
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisChannel _channel;
    private readonly JsonSerializerOptions _jsonOptions;
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
        _guildChannel = null!;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        var sub = _redis.GetSubscriber();

        await sub.SubscribeAsync(_channel, (channel, message) => {
            Logger.LogDebug("[NefariousBot:{Channel}] - Received playlist update notification: {Message}", channel, message.ToString());
            // enables token cancellation
            _ = Task.Run(async () => {
                var json = message.ToString();
                var data = JsonSerializer.Deserialize<PlaylistUpdated>(json, _jsonOptions);
                await NotifyChannel(data);
            }, token);
        });
        await base.ExecuteAsync(token);
    }

    protected override async Task OnClientReady()
    {
        if (await Client.GetChannelAsync(1297242079742922796) is IMessageChannel channel)
            _guildChannel = channel;
    }

    private async Task NotifyChannel(PlaylistUpdated? message)
    {
        if (message is not null)
        {
            var embed = new EmbedBuilder()
                .WithTitle($"'{message.PlaylistName}' has been updated!")
                .WithDescription($"*{message.UpdatedTrack.Name}* by {message.UpdatedTrack.Artists}")
                .WithUrl(message.UpdatedTrack.Url)
                .WithThumbnailUrl(message.UpdatedTrack.AlbumCoverUrl)
                .WithFooter($"Updated by: {message.UpdatedBy}")
                .WithColor(Color.Green)
                .WithCurrentTimestamp();

            await _guildChannel.SendMessageAsync(embed: embed.Build());
        }
        else
            Logger.LogError("[NefariousBotService] No message provided - message is null");
    }
}