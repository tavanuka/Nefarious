namespace Nefarious.Common.Events;

public record TrackDetails(string Name, string Artists, string Url, string Album, string AlbumCoverUrl);

// Because this thing is too generic, eventually I will do some dumb things. One of those dumb things is definitely specific events if deleted or such
public record PlaylistUpdated(string PlaylistId, string PlaylistName, string UpdatedBy, TrackDetails UpdatedTrack);
