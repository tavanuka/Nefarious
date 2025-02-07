using SpotifyAPI.Web;

namespace Nefarious.Tests.Spotify;

public static class FullPlaylistMock
{
    /// <summary>
    /// Constructs and returns a dummy sample for testing. 
    /// </summary>
    /// <returns>A populated <see cref="FullPlaylist"/> that contains a paging collection of <see cref="PlaylistTrack{T}"/> of object type <see cref="FullTrack"/>.</returns>
    public static FullPlaylist GetMockFullPlaylist() => new()
    {
        Id = "mockPlaylistId",
        Name = "My Mock Playlist",
        Description = "This is a mock playlist for testing purposes.",
        Tracks = new Paging<PlaylistTrack<IPlayableItem>>
        {
            Items =
            [
                new PlaylistTrack<IPlayableItem>
                {
                    Track = new FullTrack
                    {
                        Id = "mockTrackId1",
                        Name = "Mock Track 1",
                        Artists = [new SimpleArtist { Name = "Mock Artist 1" }],
                        Album = new SimpleAlbum { Name = "Mock Album 1" }
                    },
                    AddedAt = new DateTime(2024, 12, 25)
                },
                new PlaylistTrack<IPlayableItem>
                {
                    Track = new FullTrack
                    {
                        Id = "mockTrackId2",
                        Name = "Mock Track 2",
                        Artists = [new SimpleArtist { Name = "Mock Artist 2" }],
                        Album = new SimpleAlbum { Name = "Mock Album 2" }
                    },
                    AddedAt = new DateTime(2024, 12, 31)
                }
            ]
        }
    };
}