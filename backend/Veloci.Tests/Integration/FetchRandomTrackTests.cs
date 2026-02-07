using Veloci.Logic.Features.Cups;
using Veloci.Logic.Services.Tracks;

namespace Veloci.Tests.Integration;

public class FetchRandomTrackTests
{
    [Fact]
    [Trait ("Category", "Integration")]
    public async Task can_get_random_track()
    {
        var ts = new TrackService(new WebTrackFetcher(), null, null, null);
        var t = await ts.GetRandomTrackAsync(new TrackFilter(new TrackFilterOptions()));
    }
}
