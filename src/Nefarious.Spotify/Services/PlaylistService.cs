using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Nefarious.Spotify.Repository;

namespace Nefarious.Spotify.Services;

public interface IPlaylistService
{
    Task<bool> IsPlaylistUpdated(string playlistId);
}

[Obsolete("Will be replaced by the ICachedPlaylistRepository implementation. I think.")]
public class PlaylistService : IPlaylistService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<PlaylistService> _logger;

    public PlaylistService(IDistributedCache cache, ILogger<PlaylistService> logger, ICachedPlaylistRepository playlistRepository)
    {
        _cache = cache;
        _logger = logger;
    }
    [Obsolete("Will be replaced by the ICachedPlaylistRepository implementation. I think.")]
    public Task<bool> IsPlaylistUpdated(string playlistId) => throw new NotImplementedException();
}