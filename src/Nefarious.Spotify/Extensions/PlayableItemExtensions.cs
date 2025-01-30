using Nefarious.Common.Events;
using Nefarious.Common.Extensions;
using SpotifyAPI.Web;
using System.Diagnostics.Contracts;

namespace Nefarious.Spotify.Extensions;

public static class PlayableItemExtensions
{
    [Pure]
    public static string GetTrackId(this IPlayableItem item)
        => item switch
        {
            FullTrack fullTrack => fullTrack.Id,
            FullEpisode fullEpisode => fullEpisode.Id,
            _ => string.Empty
        };

    public static TrackDetails GetTrackDetails(this IPlayableItem item, string playlistUri)
    {
        var queryParams = new Dictionary<string, string>
        {
            // BUG: This does not really work but thankfully, not my fault.
            //  Playlist deep-linking is shitting its pants and I have no idea how spotify links tracks in a playlist.
            { "context", playlistUri }
        };
        return item switch
        {
            FullTrack fullTrack => CreateFullTrackDetails(fullTrack, queryParams),
            FullEpisode => throw new NotImplementedException("Episodes are not implemented yet."),
            _ => throw new ArgumentException("Invalid playable item type")
        };
    }

    private static TrackDetails CreateFullTrackDetails(FullTrack track, Dictionary<string, string> queryParams)
        => new(track.Name,
            track.Artists
                .Select(a => a.Name)
                .Aggregate((a, b) => $"{a}, {b}"),
            new Uri(track.ExternalUrls["spotify"])
                .AddParameters(queryParams)
                .ToString(),
            track.Album.Name,
            track.Album.Images.FirstOrDefault()?.Url ?? string.Empty
        );
}