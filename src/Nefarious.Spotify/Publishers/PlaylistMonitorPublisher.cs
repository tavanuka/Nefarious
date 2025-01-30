using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nefarious.Common.Events;
using Nefarious.Spotify.Converters;
using Nefarious.Spotify.Extensions;
using Nefarious.Spotify.Repository;
using Nefarious.Spotify.Services;
using SpotifyAPI.Web;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nefarious.Spotify.Publishers;

public class PlaylistMonitorPublisher : BackgroundService
{
    private readonly ILogger<PlaylistMonitorPublisher> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly ICachedPlaylistRepository _cachedPlaylistRepository;
    private readonly IPlaylistSubscriptionService _playlistSubscription;
    private readonly RedisChannel _channel;
    private readonly JsonSerializerOptions _jsonOptions;

    public PlaylistMonitorPublisher(ILogger<PlaylistMonitorPublisher> logger, IConnectionMultiplexer redis, ICachedPlaylistRepository cachedPlaylistRepository, IPlaylistSubscriptionService playlistSubscription)
    {
        _logger = logger;
        _redis = redis;
        _cachedPlaylistRepository = cachedPlaylistRepository;
        _playlistSubscription = playlistSubscription;

        _channel = new RedisChannel("playlist_monitor", RedisChannel.PatternMode.Literal);
        _jsonOptions = new JsonSerializerOptions
        {
            Converters =
            {
                new JsonStringEnumConverter(),
                new SystemPlayableItemConverter()
            },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("PlaylistMonitorPublisher running at: {Time}", DateTimeOffset.Now);

            var subscriber = _redis.GetSubscriber();

            var subscriptions = _playlistSubscription.GetAllSubscribers();
            foreach (var playlistId in subscriptions.Keys)
            {
                var playlistUpdated = await _cachedPlaylistRepository.IsPlaylistUpdated(playlistId);
                if (!playlistUpdated)
                    continue;

                var cachedPlaylist = await _cachedPlaylistRepository.GetCachedPlaylist(playlistId);
                var playlist = await _cachedPlaylistRepository.GetPlaylist(playlistId);

                var addedTracks = playlist
                    .GetAddedPlaylistTracks(cachedPlaylist)
                    .ToList();
                var removedTracks = playlist
                    .GetRemovedPlaylistTracks(cachedPlaylist)
                    .ToList();

                if (addedTracks.Count > 0)
                    await PublishTrackUpdates(subscriber, addedTracks, playlist.Uri!, (track, addedBy) =>
                        new TrackAddedToPlaylist(playlistId, subscriptions[playlistId], playlist.Name!, addedBy, track));

                if (removedTracks.Count > 0)
                    await PublishTrackUpdates(subscriber, removedTracks, playlist.Uri!, (track, addedBy) =>
                        new TrackRemovedFromPlaylist(playlistId, subscriptions[playlistId], playlist.Name!, addedBy, track));

                await _cachedPlaylistRepository.SavePlaylistToCache(playlist);
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }

    /// <summary>
    /// Publishes updates for a collection of playlist tracks to Redis.
    /// </summary>
    /// <param name="subscriber">The Redis subscriber used to publish messages.</param>
    /// <param name="tracks">The collection of tracks to process.</param>
    /// <param name="playlistUri">The URI of the playlist.</param>
    /// <param name="createMessage">
    /// A function that takes <see cref="TrackDetails"/> and the user who added the track,
    /// and returns a <see cref="PlaylistUpdated"/> message.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task PublishTrackUpdates(
        ISubscriber subscriber,
        IEnumerable<PlaylistTrack<IPlayableItem>> tracks,
        string playlistUri,
        Func<TrackDetails, string, PlaylistUpdated> createMessage)
    {
        foreach (var track in tracks)
        {
            var addedBy = string.IsNullOrWhiteSpace(track.AddedBy.DisplayName)
                ? track.AddedBy.Id
                : track.AddedBy.DisplayName;

            var trackDetails = track.Track.GetTrackDetails(playlistUri);

            var message = createMessage(trackDetails, addedBy);

            var jsonMessage = JsonSerializer.Serialize(message, _jsonOptions);
            await subscriber.PublishAsync(_channel, jsonMessage);
        }
    }
}