using FpvBattle.Application.Features.Tracks.Services;
using Hangfire;

namespace FpvBattle.Application.Features.Tracks.Jobs;

public class SyncTrackLookupsJob
{
    private readonly TrackLookupSync _sync;

    public SyncTrackLookupsJob(TrackLookupSync sync)
    {
        _sync = sync;
    }

    [DisableConcurrentExecution("SyncTrackLookups", 60)]
    public Task Handle(CancellationToken ct) => _sync.SyncAsync(ct);
}
