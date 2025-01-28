using Microsoft.Extensions.Caching.Distributed;
using Nefarious.Spotify.Converters;
using SpotifyAPI.Web;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nefarious.Spotify.Repository;

public class CachedPlaylistRepository : ICachedPlaylistRepository
{
    private readonly ISpotifyClient _spotifyClient;
    private readonly IDistributedCache _cache;
    private readonly ConcurrentDictionary<string, FullPlaylist> _memoryCache = new();
    private readonly JsonSerializerOptions _jsonOptions;

    public CachedPlaylistRepository(ISpotifyClient spotifyClient, IDistributedCache cache)
    {
        _spotifyClient = spotifyClient;
        _cache = cache;
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

    public async Task<FullPlaylist> GetPlaylist(string playlistId)
    {
        var cachePlaylistKey = $"playlist:{playlistId}";
        var snapshotKey = $"{cachePlaylistKey}:snapshot";

        var playlist = await GetPlaylistFromApi(playlistId);

        if (string.IsNullOrEmpty(playlist.SnapshotId))
            return playlist;

        var cachedPlaylist = await _cache.GetStringAsync(cachePlaylistKey);
        var cachedSnapshotId = await _cache.GetStringAsync(snapshotKey);

        if (!string.IsNullOrEmpty(cachedPlaylist) && cachedSnapshotId == playlist.SnapshotId)
            return JsonSerializer.Deserialize<FullPlaylist>(cachedPlaylist, _jsonOptions) ?? new FullPlaylist();

        await _cache.SetStringAsync(snapshotKey, playlist.SnapshotId);
        await _cache.SetStringAsync(cachePlaylistKey, JsonSerializer.Serialize(playlist, options: _jsonOptions));

        return playlist;
    }

    public async Task<bool> IsPlaylistUpdated(string playlistId)
    {
        var playlistKey = $"playlist:{playlistId}";
        var snapshotKey = $"{playlistKey}:snapshot";

        var cachedSnapshotId = await _cache.GetStringAsync(snapshotKey);
        var playlist = await GetPlaylist(playlistId);

        return cachedSnapshotId != playlist.SnapshotId;
    }

    /*
    TODO: eventually clear the cache to prevent any nasty overflows or stale in memory caching.
     I find this process extremely bogus and really need to figure out a better temporary caching solution persistence.
     */
    /// <summary>
    /// Sends an API call to spotify to retrieve the playlist for memory caching.
    /// </summary>
    /// <param name="playlistId">The Identifier of the playlist.</param>
    /// <returns></returns>
    private async Task<FullPlaylist> GetPlaylistFromApi(string playlistId)
    {
        if (_memoryCache.TryGetValue(playlistId, out var cachedPlaylist))
            return cachedPlaylist;

        var playlist = await _spotifyClient.Playlists.Get(playlistId);

        if (string.IsNullOrEmpty(playlist.SnapshotId))
            _memoryCache[playlistId] = playlist;

        return playlist;
    }
}