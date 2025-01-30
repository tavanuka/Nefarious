using SpotifyAPI.Web;
using System.Diagnostics.Contracts;

namespace Nefarious.Spotify.Extensions;

// I actually did tests on this. too good to be true?
public static class FullPlaylistExtensions
{
    /// <summary>
    /// Retrieves the last added track of a playlist.
    /// </summary>
    /// <param name="playlist">The playlist to retrieve the track from.</param>
    /// <returns>A <see cref="PlaylistTrack{T}"/> where <c>T</c> is <see cref="IPlayableItem"/> representing the last added track, or an empty track if none are found.</returns>
    public static PlaylistTrack<IPlayableItem> GetLastTrack(this FullPlaylist playlist) =>
        playlist.Tracks?.Items?
            .OrderByDescending(t => t.AddedAt)
            .FirstOrDefault() ?? new PlaylistTrack<IPlayableItem>();

    /// <summary>
    /// Retrieves a track or episode from the playlist by its ID.
    /// </summary>
    /// <typeparam name="T">The type of the track or episode to return, must implement <see cref="IPlayableItem"/>.</typeparam>
    /// <param name="playlist">The playlist containing the tracks or episodes.</param>
    /// <param name="trackId">The ID of the track or episode to find.</param>
    /// <returns>A <see cref="PlaylistTrack{T}"/> containing the matching track or episode, or an empty instance if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="playlist"/> is null.</exception>
    /// <remarks>
    /// The method supports both tracks <see cref="FullTrack"/> and episodes <see cref="FullEpisode"/> based on the <see cref="ItemType"/> property enum of <see cref="IPlayableItem"/>.
    /// </remarks>
    public static PlaylistTrack<T> GetTrack<T>(this FullPlaylist playlist, string trackId) where T : class, IPlayableItem =>
        playlist.Tracks?.Items?
                .FirstOrDefault(t => IsMatchingTrackOrEpisode(t.Track, trackId))
            is { Track: T fullItem } pTrack
            ? new PlaylistTrack<T>
            {
                AddedAt = pTrack.AddedAt,
                AddedBy = pTrack.AddedBy,
                IsLocal = pTrack.IsLocal,
                Track = fullItem
            }
            : new PlaylistTrack<T>();

    private static bool IsMatchingTrackOrEpisode(IPlayableItem track, string trackId) =>
        track.Type == ItemType.Track && track is FullTrack fullTrack && fullTrack.Id == trackId ||
        track.Type == ItemType.Episode && track is FullEpisode fullEpisode && fullEpisode.Id == trackId;
}