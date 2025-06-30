using System.Text.Json;
using FluentAssertions;
using Veloci.Logic.API.Dto;
using Veloci.Logic.Services;

namespace Veloci.Tests;

public class ResponseConverterTests
{
    private readonly RaceResultsConverter _converter = new();

    [Fact]
    public void can_calculate_ranks()
    {
        var json = /*language:json*/"""
                                    [
                                        {
                                          "lap_time": "56.055",
                                          "playername": "SWEEPER",
                                          "model_id": 59,
                                          "country": "UA"
                                        },
                                        {
                                          "lap_time": "56.300",
                                          "playername": "APX - BURAK",
                                          "model_id": 33,
                                          "country": "UA"
                                        },
                                        {
                                          "lap_time": "61.145",
                                          "playername": "Sarah",
                                          "model_id": 27,
                                          "country": "NL"
                                        },
                                        {
                                          "lap_time": "61.818",
                                          "playername": "FPV FPV",
                                          "model_id": 104,
                                          "country": "UA"
                                        }
                                    ]
                                    """;
        var data = JsonSerializer.Deserialize<List<TrackTimeDto>>(json);

        var times = _converter.ConvertTrackTimes(data);

        times.Should().HaveCount(3);

        var first = times[0];
        first.PlayerName.Should().Be("SWEEPER");
        first.LocalRank.Should().Be(1);
        first.GlobalRank.Should().Be(1);

        var second = times[1];
        second.PlayerName.Should().Be("APX - BURAK");
        second.LocalRank.Should().Be(2);
        second.GlobalRank.Should().Be(2);

        var third = times[2];
        third.PlayerName.Should().Be("FPV FPV");
        third.LocalRank.Should().Be(3);
        third.GlobalRank.Should().Be(4);
    }

    [Fact]
    public void can_parse_time()
    {
        var json = /*language:json*/"""
                                    [
                                        {
                                          "lap_time": "56.055",
                                          "playername": "SWEEPER",
                                          "model_id": 59,
                                          "country": "UA"
                                        }
                                    ]
                                    """;
        var data = JsonSerializer.Deserialize<List<TrackTimeDto>>(json);

        var times = _converter.ConvertTrackTimes(data);

        var first = times[0];
        first.Time.Should().Be(56055);
    }

    [Fact]
    public void fastest_time_from_two_models_is_considered()
    {
        var json = /*language:json*/"""
                                    [
                                        {
                                          "lap_time": "56.055",
                                          "playername": "SWEEPER",
                                          "model_id": 59,
                                          "country": "UA"
                                        },
                                        {
                                          "lap_time": "56.300",
                                          "playername": "SWEEPER",
                                          "model_id": 33,
                                          "country": "UA"
                                        },
                                        {
                                          "lap_time": "61.818",
                                          "playername": "FPV FPV",
                                          "model_id": 104,
                                          "country": "UA"
                                        }
                                    ]
                                    """;
        var data = JsonSerializer.Deserialize<List<TrackTimeDto>>(json);

        var times = _converter.ConvertTrackTimes(data);

        times.Should().HaveCount(2);

        var first = times[0];

        first.Time.Should().Be(56055);
        first.PlayerName.Should().Be("SWEEPER");
    }
}
