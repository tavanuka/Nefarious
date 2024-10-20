using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nefarious.Spotify.Services;
using StackExchange.Redis;

namespace Nefarious.Spotify.Publishers;

public class PlaylistMonitorPublisher : BackgroundService
{
    private readonly IPlaylistService _playlistService;
    private readonly ILogger<PlaylistMonitorPublisher> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisChannel _channel;

    public PlaylistMonitorPublisher(IPlaylistService playlistService, ILogger<PlaylistMonitorPublisher> logger, IConnectionMultiplexer redis)
    {
        _playlistService = playlistService;
        _logger = logger;
        _redis = redis;
        _channel = new RedisChannel("playlist_monitor", RedisChannel.PatternMode.Literal);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("PlaylistMonitorPublisher running at: {Time}", DateTimeOffset.Now);
            var updated = await _playlistService.IsPlaylistUpdated("03I7be2NrmPbDboNKz739w");
            if (updated)
            {
                await _redis.GetSubscriber().PublishAsync(_channel, "Playlist updated with funky music!");
            }
            else 
                await _redis.GetSubscriber().PublishAsync(_channel, "Playlist not updated...");
            
            await Task.Delay(TimeSpan.FromSeconds(25), stoppingToken);
        }
        
    }
}