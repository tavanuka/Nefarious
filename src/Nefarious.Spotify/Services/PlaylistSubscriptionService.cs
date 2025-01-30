using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Nefarious.Spotify.Services;

public class PlaylistSubscriptionService : IPlaylistSubscriptionService
{
    private readonly ILogger<PlaylistSubscriptionService> _logger;
    private readonly ConcurrentDictionary<string, HashSet<ulong>> _playlistSubscriptions = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    // TODO: add ability to cache or store them somewhere to complement in memory storage and enable persistence.
    // TODO: In distant future when this scales, I dont want ulong to be hardcoded and add an module agnostic subscriber type.
    public PlaylistSubscriptionService(ILogger<PlaylistSubscriptionService> logger)
    {
        _logger = logger;
    }

    public async Task AddSubscription(string playlistId, ulong channelId)
    {
        var semaphore = GetOrAddLock(playlistId);
        await semaphore.WaitAsync();
        try
        {
            _playlistSubscriptions.AddOrUpdate(
                playlistId,
                _ => [channelId],
                (_, subscriptions) => {
                    if (!subscriptions.Add(channelId))
                        _logger.LogWarning("[Nefarious:Spotify] Playlist Subscription Service is already subscribed to playlist '{PlaylistId}' for channel '{ChannelId}'", playlistId, channelId);
                    return subscriptions;
                }
            );
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task RemoveSubscription(string playlistId, ulong channelId)
    {
        var semaphore = GetOrAddLock(playlistId);
        await semaphore.WaitAsync();
        try
        {
            if (_playlistSubscriptions.TryGetValue(playlistId, out var subscriptions))
            {
                subscriptions.Remove(channelId);
                if (subscriptions.Count == 0)
                {
                    _playlistSubscriptions.TryRemove(playlistId, out _);
                    _locks.TryRemove(playlistId, out _);
                }
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    public List<string> GetPlaylistIds()
        => _playlistSubscriptions.Keys.ToList();

    public Dictionary<string, List<ulong>> GetAllSubscribers()
        => _playlistSubscriptions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList());

    public List<ulong> GetSubscribers(string playlistId)
        => _playlistSubscriptions.TryGetValue(playlistId, out var subscriptions)
            ? subscriptions.ToList()
            : [];

    private SemaphoreSlim GetOrAddLock(string playlistId) =>
        _locks.GetOrAdd(playlistId, _ => new SemaphoreSlim(1, 1));
}