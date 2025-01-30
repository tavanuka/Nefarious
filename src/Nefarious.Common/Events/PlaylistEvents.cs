namespace Nefarious.Common.Events;

public record TrackAddedToPlaylist(string PlaylistId, List<ulong> Subscribers, string PlaylistName, string UpdatedBy, TrackDetails UpdatedTrack)
    : PlaylistUpdated(PlaylistId, Subscribers, PlaylistName, UpdatedBy, UpdatedTrack);

public record TrackRemovedFromPlaylist(string PlaylistId, List<ulong> Subscribers, string PlaylistName, string UpdatedBy, TrackDetails UpdatedTrack)
    : PlaylistUpdated(PlaylistId, Subscribers, PlaylistName, UpdatedBy, UpdatedTrack);