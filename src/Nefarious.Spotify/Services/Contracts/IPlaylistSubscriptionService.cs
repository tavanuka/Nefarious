namespace Nefarious.Spotify.Services;

public interface IPlaylistSubscriptionService
{
    Task AddSubscription(string playlistId, ulong channelId);
    Task RemoveSubscription(string playlistId, ulong channelId);
    List<ulong> GetSubscribers(string playlistId);
    Dictionary<string, List<ulong>> GetAllSubscribers();
    List<string> GetPlaylistIds();
}