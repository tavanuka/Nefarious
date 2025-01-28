using SpotifyAPI.Web;

namespace Nefarious.Spotify.Repository;

public interface ICachedPlaylistRepository
{
    Task<FullPlaylist> GetPlaylist(string playlistId);
    Task<bool> IsPlaylistUpdated(string playlistId);
}