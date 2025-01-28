using Nefarious.Spotify.Extensions;
using SpotifyAPI.Web;

namespace Nefarious.Tests.Spotify;

public class FullPlaylistExtensionsTests
{
    [Fact]
    public void GetLastTrack_ShouldReturn_NotEmptyPlaylistTrack()
    {
        // Arrange
        var fullPlaylist = FullPlaylistMock.GetMockFullPlaylist();
        var emptyTrack = new PlaylistTrack<IPlayableItem>();

        // Act 
        var result = fullPlaylist.GetLastTrack();

        // Assert
        Assert.NotEqual(emptyTrack, result);
        Assert.NotNull(result.Track);
    }

    [Fact]
    public void GetLastTrack_ShouldReturn_LastAddedTrack()
    {
        // Arrange
        var fullPlaylist = FullPlaylistMock.GetMockFullPlaylist();
        var expectedTime = new DateTime(2024, 12, 31);

        // Act 
        var result = fullPlaylist.GetLastTrack();

        // Assert
        Assert.Equal(expectedTime, result.AddedAt);
    }

    [Fact]
    public void GetTrack_ShouldReturn_CorrectFullTrackById()
    {
        // Arrange
        var fullPlaylist = FullPlaylistMock.GetMockFullPlaylist();
        var expectedTrack = new FullTrack
        {
            Id = "mockTrackId1",
            Name = "Mock Track 1",
            Artists = [new SimpleArtist { Name = "Mock Artist 1" }],
            Album = new SimpleAlbum { Name = "Mock Album 1" }
        };
        
        // Act
        var result = fullPlaylist.GetTrack<FullTrack>("mockTrackId1");

        // Assert
        Assert.Equivalent(expectedTrack, result.Track, strict: true);
    }
}