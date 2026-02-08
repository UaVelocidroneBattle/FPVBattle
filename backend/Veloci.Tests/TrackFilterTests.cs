using FluentAssertions;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Services.Tracks;
using Veloci.Logic.Services.Tracks.Models;

namespace Veloci.Tests;

public class TrackFilterTests
{
    private static ParsedTrackModel CreateTrack(string name, int sceneId = 1, int type = 0)
    {
        var map = new ParsedMapModel { Id = sceneId, Name = sceneId.ToString() };
        var track = new ParsedTrackModel { Name = name, Map = map, Type = type };
        return track;
    }

    private static ParsedMapModel CreateMap(int sceneId, params string[] trackNames)
    {
        var map = new ParsedMapModel { Id = sceneId, Name = sceneId.ToString() };
        map.Tracks = trackNames.Select(n => new ParsedTrackModel { Name = n, Map = map }).ToList();
        return map;
    }

    public class BlacklistPatterns
    {
        private readonly TrackFilter _filter;

        public BlacklistPatterns()
        {
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
        public void excludes_blacklisted_tracks(string trackName)
        {
            var track = CreateTrack(trackName);
            _filter.IsTrackSuitable(track).Should().BeFalse();
        }

        [Theory]
        [InlineData("TBS Live VI Race 2")]
        public void allows_non_blacklisted_tracks(string trackName)
        {
            var track = CreateTrack(trackName);
            _filter.IsTrackSuitable(track).Should().BeTrue();
        }
    }

    public class WhitelistScenes
    {
        private readonly TrackFilter _filter;

        public WhitelistScenes()
        {
            var options = new TrackFilterOptions
            {
                WhitelistScenes = new Dictionary<int, string>
                {
                    { 3, "Hangar" },
                    { 7, "Industrial Wasteland" }
                }
            };
            _filter = new TrackFilter(options);
        }

        [Fact]
        public void allows_tracks_from_whitelisted_scenes()
        {
            var maps = new List<ParsedMapModel> { CreateMap(3, "Race 1", "Race 2") };
            _filter.GetSuitableTracks(maps).Should().HaveCount(2);
        }

        [Fact]
        public void excludes_tracks_from_non_whitelisted_scenes()
        {
            var maps = new List<ParsedMapModel> { CreateMap(99, "Race 1") };
            _filter.GetSuitableTracks(maps).Should().BeEmpty();
        }

        [Fact]
        public void maps_scene_names_from_whitelist()
        {
            var maps = new List<ParsedMapModel> { CreateMap(3, "Race 1") };
            var tracks = _filter.GetSuitableTracks(maps);
            tracks[0].Map.Name.Should().Be("Hangar");
        }
    }

    public class WhitelistTrackTypes
    {
        private readonly TrackFilter _filter;

        public WhitelistTrackTypes()
        {
            var options = new TrackFilterOptions
            {
                WhitelistTrackTypes = new List<int> { 7, 8, 9 }
            };
            _filter = new TrackFilter(options);
        }

        [Fact]
        public void allows_whitelisted_track_types()
        {
            var track = CreateTrack("Whoop Race 1", type: 7);
            _filter.IsTrackSuitable(track).Should().BeTrue();
        }

        [Fact]
        public void excludes_non_whitelisted_track_types()
        {
            var track = CreateTrack("Regular Race", type: 1);
            _filter.IsTrackSuitable(track).Should().BeFalse();
        }
    }

    public class BlacklistTrackTypes
    {
        private readonly TrackFilter _filter;

        public BlacklistTrackTypes()
        {
            var options = new TrackFilterOptions
            {
                BlacklistTrackTypes = new List<int> { 7, 8, 9 }
            };
            _filter = new TrackFilter(options);
        }

        [Fact]
        public void excludes_blacklisted_track_types()
        {
            var track = CreateTrack("Some Track", type: 7);
            _filter.IsTrackSuitable(track).Should().BeFalse();
        }

        [Fact]
        public void allows_non_blacklisted_track_types()
        {
            var track = CreateTrack("Some Track", type: 1);
            _filter.IsTrackSuitable(track).Should().BeTrue();
        }
    }

    public class CombinedFilters
    {
        [Fact]
        public void applies_all_filters_together()
        {
            var options = new TrackFilterOptions
            {
                WhitelistScenes = new Dictionary<int, string> { { 3, "Hangar" } },
                BlacklistPatterns = new List<string> { "Freestyle" },
                BlacklistTrackTypes = new List<int> { 9 }
            };
            var filter = new TrackFilter(options);

            var maps = new List<ParsedMapModel>
            {
                CreateMap(3, "Race 1", "Freestyle Run"),
                CreateMap(99, "Race 2")
            };
            // Add a track with blacklisted type to scene 3
            maps[0].Tracks.Add(new ParsedTrackModel { Name = "Race Blacklisted Type", Map = maps[0], Type = 9 });

            var result = filter.GetSuitableTracks(maps);

            // Only "Race 1" passes: scene 99 excluded, "Freestyle Run" blacklisted by name, type 9 blacklisted
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Race 1");
            result[0].Map.Name.Should().Be("Hangar");
        }

        [Fact]
        public void no_filters_allows_everything()
        {
            var filter = new TrackFilter(new TrackFilterOptions());
            var maps = new List<ParsedMapModel> { CreateMap(42, "Any Track") };
            filter.GetSuitableTracks(maps).Should().HaveCount(1);
        }
    }
}
