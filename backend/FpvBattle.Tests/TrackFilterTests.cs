using FluentAssertions;
using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Services.Tracks;
using FpvBattle.Domain;

namespace FpvBattle.Tests;

public class TrackFilterTests
{
    private static Track CreateTrack(string name, int sceneId = 1, int? type = 0) => new()
    {
        Name = name,
        Type = type,
        Scene = new Scene { SceneId = sceneId, Name = $"Scene {sceneId}" }
    };

    [Theory]
    [InlineData("Betafpv 2s Power Finals")]
    [InlineData("NBD MICRO SERIES RACE 8")]
    [InlineData("NBD mIcrO SERIES RACE 8")]
    public void IsTrackSuitable_ShouldBeFalse_WhenNameMatchesBlacklistPattern(string trackName)
    {
        var filter = new TrackFilter(new TrackFilterOptions { BlacklistPatterns = ["Betafpv", "Micro", "Whoop"] });

        filter.IsTrackSuitable(CreateTrack(trackName)).Should().BeFalse();
    }

    [Fact]
    public void IsTrackSuitable_ShouldBeTrue_WhenNameMatchesNoBlacklistPattern()
    {
        var filter = new TrackFilter(new TrackFilterOptions { BlacklistPatterns = ["Betafpv", "Micro", "Whoop"] });

        filter.IsTrackSuitable(CreateTrack("TBS Live VI Race 2")).Should().BeTrue();
    }

    [Theory]
    [InlineData(3, true)]
    [InlineData(99, false)]
    public void IsTrackSuitable_ShouldFollowSceneWhitelist_ByVelocidroneSceneId(int sceneId, bool expected)
    {
        var filter = new TrackFilter(new TrackFilterOptions
        {
            WhitelistScenes = new Dictionary<int, string> { [3] = "Hangar", [7] = "Industrial Wasteland" }
        });

        filter.IsTrackSuitable(CreateTrack("Race 1", sceneId)).Should().Be(expected);
    }

    [Theory]
    [InlineData(7, true)]
    [InlineData(1, false)]
    [InlineData(null, false)]
    public void IsTrackSuitable_ShouldFollowTypeWhitelist(int? type, bool expected)
    {
        var filter = new TrackFilter(new TrackFilterOptions { WhitelistTrackTypes = [7, 8, 9] });

        filter.IsTrackSuitable(CreateTrack("Race", type: type)).Should().Be(expected);
    }

    [Theory]
    [InlineData(7, false)]
    [InlineData(1, true)]
    [InlineData(null, false)]
    public void IsTrackSuitable_ShouldFollowTypeBlacklist(int? type, bool expected)
    {
        var filter = new TrackFilter(new TrackFilterOptions { BlacklistTrackTypes = [7, 8, 9] });

        filter.IsTrackSuitable(CreateTrack("Race", type: type)).Should().Be(expected);
    }

    [Fact]
    public void GetSuitableTracks_ShouldApplyEveryFilter_WhenCombined()
    {
        var filter = new TrackFilter(new TrackFilterOptions
        {
            WhitelistScenes = new Dictionary<int, string> { [3] = "Hangar" },
            BlacklistPatterns = ["Freestyle"],
            BlacklistTrackTypes = [9]
        });

        var tracks = new[]
        {
            CreateTrack("Race 1", sceneId: 3),
            CreateTrack("Freestyle Run", sceneId: 3),
            CreateTrack("Race Blacklisted Type", sceneId: 3, type: 9),
            CreateTrack("Race 2", sceneId: 99)
        };

        filter.GetSuitableTracks(tracks).Select(t => t.Name).Should().Equal("Race 1");
    }

    [Fact]
    public void GetSuitableTracks_ShouldKeepEveryTrack_WhenNoFiltersAreConfigured()
    {
        var filter = new TrackFilter(new TrackFilterOptions());

        filter.GetSuitableTracks([CreateTrack("Any Track", sceneId: 42, type: null)]).Should().HaveCount(1);
    }
}
