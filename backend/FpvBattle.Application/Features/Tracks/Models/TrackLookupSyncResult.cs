namespace FpvBattle.Application.Features.Tracks.Models;

public sealed record TrackLookupSyncResult(LookupSyncCounts Scenes, LookupSyncCounts Tracks);

public sealed class LookupSyncCounts
{
    public int Added { get; internal set; }
    public int Updated { get; internal set; }
    public int Restored { get; internal set; }
    public int Deleted { get; internal set; }
}
