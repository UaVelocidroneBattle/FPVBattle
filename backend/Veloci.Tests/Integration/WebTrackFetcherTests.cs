using FluentAssertions;
using Veloci.Logic.Services.Tracks;

namespace Veloci.Tests.Integration;

public class WebTrackFetcherTests
{
    [Fact]
    [Trait ("Category", "Integration")]
    public async Task can_fetch_track_via_web()
    {
        var fetcher = new WebTrackFetcher();
        var maps = await fetcher.FetchMapsAsync();

        maps.Should().NotBeEmpty();
        maps.Should().OnlyContain(m => !string.IsNullOrWhiteSpace(m.Name) && m.Id > 0);
        maps.SelectMany(m => m.Tracks).Should().NotBeEmpty().And.OnlyContain(t => !string.IsNullOrWhiteSpace(t.Name) && t.Id > 0);
    }

    [Fact]
    [Trait ("Category", "Integration")]
    public async Task can_fetch_track_via_api()
    {
        var fetcher = new ApiTrackFetcher();
        var maps = await fetcher.FetchMapsAsync();
    }
}
