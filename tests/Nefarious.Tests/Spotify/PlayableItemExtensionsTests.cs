using Nefarious.Spotify.Extensions;
using NSubstitute;
using SpotifyAPI.Web;

namespace Nefarious.Tests.Spotify;

public class PlayableItemExtensionsTests
{
    [Fact]
    public void GetTrackId_ShouldReturn_TrackId()
    {
        // Arrange
        var expectedTrack = new FullTrack
        {
            Id = "mockTrackId1",
            Name = "Mock Track 1",
            Artists = [new SimpleArtist { Name = "Mock Artist 1" }],
            Album = new SimpleAlbum { Name = "Mock Album 1" }
        };
        var expectedEpisode = new FullEpisode
        {
            Id = "mockEpisodeId1",
            Name = "Mock Episode 1",
            Description = "This is a mock episode for testing."
        };

        // Act
        var actualTrack = expectedTrack.GetTrackId();
        var actualEpisode = expectedEpisode.GetTrackId();

        // Assert 
        Assert.Equal(expectedTrack.Id, actualTrack);
        Assert.Equal(expectedEpisode.Id, actualEpisode);
    }

    [Fact]
    public void GetTrackDetails_ShouldReturn_CorrectTrackDetails()
    {
        // Arrange
        var fullTrack = new FullTrack
        {
            Name = "Mock Track",
            Artists = [new SimpleArtist { Name = "Artist 1" }, new SimpleArtist { Name = "Artist 2" }],
            ExternalUrls = new Dictionary<string, string> { { "spotify", "https://open.spotify.com/track/mockTrackId" } },
            Album = new SimpleAlbum
            {
                Name = "Mock Album",
                Images = [new Image { Url = "https://mockimage.com/album.jpg", Width = 640, Height = 640 }]
            }
        };
        const string playlistUri = "spotify:playlist:mockPlaylistId";

        // Act
        var actual = fullTrack.GetTrackDetails(playlistUri);

        // Assert
        var uri = new Uri(actual.Url);
        var queryParams = Uri.UnescapeDataString(uri.Query);

        Assert.Equal("Mock Track", actual.Name);
        Assert.Equal("Artist 1, Artist 2", actual.Artists);
        Assert.Contains($"context={playlistUri}", queryParams);
        Assert.Equal("Mock Album", actual.Album);
        Assert.Equal("https://mockimage.com/album.jpg", actual.AlbumCoverUrl);
    }

    [Fact]
    public void GetTrackDetails_ShouldSetEmptyStringWhen_AlbumCoverUrl_IsEmpty()
    {
        // Arrange
        var fullTrack = new FullTrack
        {
            Name = "Mock Track",
            Artists = [new SimpleArtist { Name = "Artist 1" }],
            ExternalUrls = new Dictionary<string, string> { { "spotify", "https://open.spotify.com/track/mockTrackId" } },
            Album = new SimpleAlbum { Name = "Mock Album", Images = [] }
        };
        const string playlistUri = "spotify:playlist:mockPlaylistId";

        // Act 
        var actual = fullTrack.GetTrackDetails(playlistUri).AlbumCoverUrl;

        // Assert
        Assert.Equal(string.Empty, actual);
    }

    [Fact]
    public void GetTrackDetails_ShouldThrowWhen_IPlayableItem_IsInvalid()
    {
        // Arrange
        var invalidItem = Substitute.For<IPlayableItem>();
        const string playlistUri = "spotify:playlist:mockPlaylistId";

        // Act and ASSert
        var ex = Assert.Throws<ArgumentException>(() => invalidItem.GetTrackDetails(playlistUri));
        Assert.Equal("Invalid playable item type", ex.Message);
    }
}