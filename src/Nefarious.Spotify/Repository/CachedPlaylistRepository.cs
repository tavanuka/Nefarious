using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Nefarious.Spotify.Converters;
using SpotifyAPI.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nefarious.Spotify.Repository;

public class CachedPlaylistRepository : ICachedPlaylistRepository
{
    private readonly ILogger<CachedPlaylistRepository> _logger;
    private readonly ISpotifyClient _spotifyClient;
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;

    public CachedPlaylistRepository(ILogger<CachedPlaylistRepository> logger, ISpotifyClient spotifyClient, IDistributedCache cache)
    {
        _logger = logger;
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

        var playlist = await _spotifyClient.Playlists.Get(playlistId);

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
        var snapshotId = await GetPlaylistSnapshotId(playlistId);

        if (!string.IsNullOrEmpty(snapshotId))
            return cachedSnapshotId != snapshotId;

        _logger.LogError("[Nefarious:Spotify] snapshot id returned invalid for '{PlaylistId}'", playlistId);
        return false;
    }

    private async Task<string> GetPlaylistSnapshotId(string playlistId)
        => (await _spotifyClient.Playlists.Get(playlistId, new PlaylistGetRequest { Fields = { "snapshot_id" } }))
            .SnapshotId ?? string.Empty;
}