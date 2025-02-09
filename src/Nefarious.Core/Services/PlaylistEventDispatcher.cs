using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nefarious.Common.Events;
using NetCord;
using NetCord.Rest;
using StackExchange.Redis;
using System.Text.Json;

namespace Nefarious.Core.Services;

public class PlaylistEventDispatcher : BackgroundService
{
    private readonly RestClient _client;
    private readonly ILogger<PlaylistEventDispatcher> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisChannel _channel;
    private readonly JsonSerializerOptions _jsonOptions;

    public PlaylistEventDispatcher(
        RestClient client,
        ILogger<PlaylistEventDispatcher> logger,
        IConnectionMultiplexer redis)
    {
        _client = client;
        _redis = redis;
        _logger = logger;
        _channel = new RedisChannel("playlist_monitor", RedisChannel.PatternMode.Literal);
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        var sub = _redis.GetSubscriber();

        await sub.SubscribeAsync(_channel, (channel, message) => {
            _logger.LogDebug("[NefariousBot:{Channel}] - Received playlist update notification: {Message}", channel, message.ToString());
            // enables token cancellation
            _ = Task.Run(async () => {
                var json = message.ToString();
                var data = JsonSerializer.Deserialize<PlaylistUpdated>(json, _jsonOptions);
                await NotifyChannel(data, token);
            }, token);

        });

        await Task.Delay(Timeout.Infinite, token);
    }

    private async Task NotifyChannel(PlaylistUpdated? @event, CancellationToken ct)
    {
        if (@event is not null)
        {
            var color = @event switch
            {
                TrackAddedToPlaylist => new Color(System.Drawing.Color.Green.ToArgb()),
                TrackRemovedFromPlaylist => new Color(System.Drawing.Color.Red.ToArgb()),
                _ => new Color(System.Drawing.Color.DarkOrange.ToArgb())
            };

            var msg = new MessageProperties();
            var embed = new EmbedProperties()
                .WithTitle($"'{@event.PlaylistName}' has been updated!")
                .WithDescription($"*{@event.UpdatedTrack.Name}* by {@event.UpdatedTrack.Artists}")
                .WithUrl(@event.UpdatedTrack.Url)
                .WithThumbnail(@event.UpdatedTrack.AlbumCoverUrl)
                .WithFooter(new EmbedFooterProperties()
                    .WithText($"Updated by: {@event.UpdatedBy}"))
                .WithColor(color)
                .WithTimestamp(DateTimeOffset.UtcNow);

            msg.AddEmbeds(embed);
            foreach (var channelId in @event.Subscribers)
            {
                if (await _client.GetChannelAsync(channelId, cancellationToken: ct) is not TextChannel channel)
                    continue;

                await channel.SendMessageAsync(msg, cancellationToken: ct);
            }
        }
        else
            _logger.LogError("[NefariousBotService] No message provided - message is null");
    }
}