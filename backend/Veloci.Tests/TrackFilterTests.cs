using FluentAssertions;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Services.Tracks;
using Veloci.Logic.Services.Tracks.Models;

namespace Veloci.Tests;

public class TrackFilterTests
{
    private readonly TrackFilter _filter;

    public TrackFilterTests()
    {
        // Create filter with 5-inch blacklist patterns
        var options = new TrackFilterOptions
        {
            BlacklistPatterns = new List<string>
            {
                "Pylons", "Freestyle", "Betafpv", "Beta 2S", "Micro",
                "NewBeeDrone", "NBD", "Toothpick", "Trainer", "Whoop"
            }
        };
        _filter = new TrackFilter(options);
    }

    [Theory]
    [InlineData("Betafpv 2s Power Finals")]
    [InlineData("NBD MICRO SERIES RACE 8")]
    [InlineData("NBD mIcrO SERIES RACE 8")]
    public void not_suitable_for_5inch(string trackName)
    {
        var track = new ParsedTrackModel {Name = trackName};

        _filter.IsTrackSuitable(track).Should().BeFalse($"{track} is not good for racing quad");
    }

    [Theory]
    [InlineData("TBS Live VI Race 2")]
    public void suitable_for_5inch(string trackName)
    {
        var track = new ParsedTrackModel {Name = trackName};

        _filter.IsTrackSuitable(track).Should().BeTrue($"{track} is good for racing quad");
    }
}
