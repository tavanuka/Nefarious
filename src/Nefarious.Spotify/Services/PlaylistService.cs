using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;

namespace Nefarious.Spotify.Services;

public interface IPlaylistService
{
    Task<bool> IsPlaylistUpdated(string playlistId);
}

public class PlaylistService : IPlaylistService
{
    private readonly ISpotifyClient _spotifyClient;
    private readonly IDistributedCache _cache;
    private readonly ILogger<PlaylistService> _logger;

    public PlaylistService(ISpotifyClient spotifyClient, IDistributedCache cache, ILogger<PlaylistService> logger)
    {
        _spotifyClient = spotifyClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsPlaylistUpdated(string playlistId)
    {
        var playlist = await _spotifyClient.Playlists.Get(playlistId);
        var cacheTrackCount = await _cache.GetStringAsync($"playlist:{playlistId}:track_count");
        
        if (int.TryParse(cacheTrackCount, out var cachedTrackCount) && cachedTrackCount == playlist.Tracks?.Total)
            return false;
        
        await _cache.SetStringAsync($"playlist:{playlistId}:track_count", playlist.Tracks?.Total.ToString() ?? "0");
        return true;
    }
    
}