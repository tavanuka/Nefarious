using Microsoft.Extensions.Logging;
using Nefarious.Spotify.Services;
using NSubstitute;

namespace Nefarious.Tests.Spotify;

public class PlaylistSubscriptionServiceTests
{
    private readonly PlaylistSubscriptionService _sut;
    private readonly TestableLogger<PlaylistSubscriptionService> _loggerMock;

    public PlaylistSubscriptionServiceTests()
    {
        _loggerMock = Substitute.For<TestableLogger<PlaylistSubscriptionService>>();
        _sut = new PlaylistSubscriptionService(_loggerMock);
    }

    [Fact]
    public async Task AddSubscription_ShouldAdd_PlaylistSubscriptionForChannel()
    {
        // Arrange
        const string playlistId = "fashf2fsa";
        const ulong channelId = 1235135257899UL;

        // Act
        await _sut.AddSubscription(playlistId, channelId);

        // Assert
        var subscribers = _sut.GetSubscribers(playlistId);
        Assert.Contains(channelId, subscribers);
    }

    [Fact]
    public async Task AddSubscription_ShouldNotAdd_DuplicatePlaylistSubscriptionForChannel()
    {
        // Arrange
        const string playlistId = "fashf2fsa";
        const ulong channelId = 1235135257899UL;
        var logMessage = $"[Nefarious:Spotify] Playlist Subscription Service is already subscribed to playlist '{playlistId}' for channel '{channelId}'";

        // Act
        await _sut.AddSubscription(playlistId, channelId);
        await _sut.AddSubscription(playlistId, channelId);

        // Assert
        var subscribers = _sut.GetSubscribers(playlistId);

        Assert.Single(subscribers);
        _loggerMock.Received(1).Log(LogLevel.Warning,
            Arg.Any<EventId>(),
            logMessage,
            Arg.Any<Exception>());
    }

    [Fact]
    public async Task RemoveSubscription_ShouldRemove_PlaylistSubscriptionForChannel()
    {
        // Arrange
        const string playlistId = "fashf2fsa";
        const ulong channelId = 1235135257899UL;
        await _sut.AddSubscription(playlistId, channelId);

        // Act
        await _sut.RemoveSubscription(playlistId, channelId);

        // Assert
        var subscribers = _sut.GetSubscribers(playlistId);
        Assert.Empty(subscribers);
    }

    [Fact]
    public async Task RemoveSubscription_ShouldRemovePlaylistKey_WhenNoSubscribersExist()
    {
        // Arrange
        const string playlistId = "fashf2fsa";
        const ulong channelId = 1235135257899UL;
        await _sut.AddSubscription(playlistId, channelId);

        // Act
        await _sut.RemoveSubscription(playlistId, channelId);

        // Assert
        var playlists = _sut.GetPlaylistIds();
        Assert.DoesNotContain(playlistId, playlists);
    }

    [Fact]
    public async Task GetPlaylistIds_ShouldReturn_CorrectPlaylistIds()
    {
        // Arrange
        const string expectedId1 = "fashf2fsa";
        const string expectedId2 = "gashf2sfsa";
        const ulong channelId = 1235135257899UL;

        await _sut.AddSubscription(expectedId1, channelId);
        await _sut.AddSubscription(expectedId2, channelId);

        // Act
        var actual = _sut.GetPlaylistIds();

        // Assert
        Assert.Contains(expectedId1, actual);
        Assert.Contains(expectedId2, actual);
    }

    [Fact]
    public async Task GetAllSubscribers_ShouldReturn_CorrectSubscribers()
    {
        // Arrange
        var expectedSubscriptions = new Dictionary<string, List<ulong>>
        {
            { "fashf2fsa", [123513525799UL, 49512335257899UL] },
            { "gashf2sfsa", [4951235165257899UL] }
        };

        foreach (var (playlistId, channelIds) in expectedSubscriptions)
        foreach (var channelId in channelIds)
            await _sut.AddSubscription(playlistId, channelId);

        // Act
        var actualSubscriptions = _sut.GetAllSubscribers();

        // Assert
        foreach (var (playlistId, expectedChannelIds) in expectedSubscriptions)
        {
            Assert.Contains(playlistId, actualSubscriptions.Keys);

            foreach (var channelId in expectedChannelIds)
                Assert.Contains(channelId, actualSubscriptions[playlistId]);
        }
    }
}
