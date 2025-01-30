using System.Text.Json.Serialization;

namespace Nefarious.Common.Events;

public record TrackDetails(string Name, string Artists, string Url, string Album, string AlbumCoverUrl);

// Because this thing is too generic, eventually I will do some dumb things. One of those dumb things is definitely specific events if deleted or such
[JsonDerivedType(typeof(PlaylistUpdated), "playlistUpdated")]
[JsonDerivedType(typeof(TrackAddedToPlaylist), "trackAdded")]
[JsonDerivedType(typeof(TrackRemovedFromPlaylist), "trackRemoved")]
public record PlaylistUpdated(string PlaylistId, List<ulong> Subscribers, string PlaylistName, string UpdatedBy, TrackDetails UpdatedTrack);

public record TrackAddedToPlaylist(string PlaylistId, List<ulong> Subscribers, string PlaylistName, string UpdatedBy, TrackDetails UpdatedTrack)
    : PlaylistUpdated(PlaylistId, Subscribers, PlaylistName, UpdatedBy, UpdatedTrack);

public record TrackRemovedFromPlaylist(string PlaylistId, List<ulong> Subscribers, string PlaylistName, string UpdatedBy, TrackDetails UpdatedTrack)
    : PlaylistUpdated(PlaylistId, Subscribers, PlaylistName, UpdatedBy, UpdatedTrack);