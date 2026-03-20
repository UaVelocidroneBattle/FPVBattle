using System.Linq.Expressions;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.API.Dto;
using Veloci.Logic.Services;

namespace Veloci.Tests;

public class ResponseConverterTests
{
    private readonly RaceResultsConverter _converter = CreateConverter();

    [Fact]
    public async Task can_calculate_ranks()
    {
        var json = /*language:json*/"""
                                    [
                                        {
                                          "lap_time": "56.055",
                                          "playername": "SWEEPER",
                                          "model_name": "5inch",
                                          "country": "UA",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:00:00Z",
                                          "updated_at": "2026-01-01T10:00:00Z",
                                          "user_id": 1001
                                        },
                                        {
                                          "lap_time": "56.300",
                                          "playername": "APX - BURAK",
                                          "model_name": "5inch",
                                          "country": "UA",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:01:00Z",
                                          "updated_at": "2026-01-01T10:01:00Z",
                                          "user_id": 1002
                                        },
                                        {
                                          "lap_time": "61.145",
                                          "playername": "Sarah",
                                          "model_name": "tinywhoop",
                                          "country": "NL",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:02:00Z",
                                          "updated_at": "2026-01-01T10:02:00Z",
                                          "user_id": 1003
                                        },
                                        {
                                          "lap_time": "61.818",
                                          "playername": "FPV FPV",
                                          "model_name": "cinelifter",
                                          "country": "UA",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:03:00Z",
                                          "updated_at": "2026-01-01T10:03:00Z",
                                          "user_id": 1004
                                        }
                                    ]
                                    """;
        var data = JsonSerializer.Deserialize<List<TrackTimeDto>>(json);
        data.Should().NotBeNull();

        var times = await _converter.ConvertTrackTimesAsync(data!, []);

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
        third.GlobalRank.Should().Be(4); // Sarah (NL) was #3 globally; FPV FPV retains original API rank
    }

    [Fact]
    public async Task can_parse_time()
    {
        var json = /*language:json*/"""
                                    [
                                        {
                                          "lap_time": "56.055",
                                          "playername": "SWEEPER",
                                          "model_name": "5inch",
                                          "country": "UA",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:00:00Z",
                                          "updated_at": "2026-01-01T10:00:00Z",
                                          "user_id": 1001
                                        }
                                    ]
                                    """;
        var data = JsonSerializer.Deserialize<List<TrackTimeDto>>(json);
        data.Should().NotBeNull();

        var times = await _converter.ConvertTrackTimesAsync(data!, []);

        var first = times[0];
        first.Time.Should().Be(56055);
    }

    [Fact]
    public async Task fastest_time_from_two_models_is_considered()
    {
        var json = /*language:json*/"""
                                    [
                                        {
                                          "lap_time": "56.055",
                                          "playername": "SWEEPER",
                                          "model_name": "5inch",
                                          "country": "UA",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:00:00Z",
                                          "updated_at": "2026-01-01T10:00:00Z",
                                          "user_id": 1001
                                        },
                                        {
                                          "lap_time": "56.300",
                                          "playername": "SWEEPER",
                                          "model_name": "3.5inch",
                                          "country": "UA",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:01:00Z",
                                          "updated_at": "2026-01-01T10:01:00Z",
                                          "user_id": 1001
                                        },
                                        {
                                          "lap_time": "61.818",
                                          "playername": "FPV FPV",
                                          "model_name": "cinelifter",
                                          "country": "UA",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:02:00Z",
                                          "updated_at": "2026-01-01T10:02:00Z",
                                          "user_id": 1002
                                        }
                                    ]
                                    """;
        var data = JsonSerializer.Deserialize<List<TrackTimeDto>>(json);
        data.Should().NotBeNull();

        var times = await _converter.ConvertTrackTimesAsync(data!, []);

        times.Should().HaveCount(2);

        var first = times[0];

        first.Time.Should().Be(56055);
        first.PlayerName.Should().Be("SWEEPER");
    }

    [Fact]
    public async Task blacklisted_country_is_excluded_even_when_pilot_is_whitelisted()
    {
        var json = /*language:json*/"""
                                    [
                                        {
                                          "lap_time": "56.055",
                                          "playername": "SWEEPER",
                                          "model_name": "5inch",
                                          "country": "UA",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:00:00Z",
                                          "updated_at": "2026-01-01T10:00:00Z",
                                          "user_id": 1001
                                        },
                                        {
                                          "lap_time": "56.300",
                                          "playername": "RU_PILOT",
                                          "model_name": "5inch",
                                          "country": "RU",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:01:00Z",
                                          "updated_at": "2026-01-01T10:01:00Z",
                                          "user_id": 1002
                                        }
                                    ]
                                    """;
        var data = JsonSerializer.Deserialize<List<TrackTimeDto>>(json);
        data.Should().NotBeNull();

        var converter = CreateConverter(whitelistedPilots: ["RU_PILOT"], blacklistedCountries: ["RU"]);
        var times = await converter.ConvertTrackTimesAsync(data!, []);

        times.Should().HaveCount(1);
        times[0].PlayerName.Should().Be("SWEEPER");
    }

    [Fact]
    public async Task quad_class_filter_excludes_disallowed_models()
    {
        var json = /*language:json*/"""
                                    [
                                        {
                                          "lap_time": "56.055",
                                          "playername": "SWEEPER",
                                          "model_name": "Hornet",
                                          "country": "UA",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:00:00Z",
                                          "updated_at": "2026-01-01T10:00:00Z",
                                          "user_id": 1001
                                        },
                                        {
                                          "lap_time": "57.000",
                                          "playername": "WHOOP_PILOT",
                                          "model_name": "Mobula 6",
                                          "country": "UA",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:01:00Z",
                                          "updated_at": "2026-01-01T10:01:00Z",
                                          "user_id": 1002
                                        }
                                    ]
                                    """;

        var models = new[]
        {
            new QuadModel { Id = 1, Name = "Hornet", Class = QuadClasses.Race },
            new QuadModel { Id = 2, Name = "Mobula 6", Class = QuadClasses.Micro }
        };

        var converter = CreateConverter(quadModels: models);
        var times = await converter.ConvertTrackTimesAsync(
            JsonSerializer.Deserialize<List<TrackTimeDto>>(json)!,
            allowedQuadClasses: [QuadClasses.Micro]);

        times.Should().HaveCount(1);
        times[0].PlayerName.Should().Be("WHOOP_PILOT");
    }

    [Fact]
    public async Task quad_class_filter_allows_unknown_models_through()
    {
        var json = /*language:json*/"""
                                    [
                                        {
                                          "lap_time": "56.055",
                                          "playername": "SWEEPER",
                                          "model_name": "Brand New Model",
                                          "country": "UA",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:00:00Z",
                                          "updated_at": "2026-01-01T10:00:00Z",
                                          "user_id": 1001
                                        }
                                    ]
                                    """;

        var converter = CreateConverter(quadModels: []); // empty — model not in DB yet
        var times = await converter.ConvertTrackTimesAsync(
            JsonSerializer.Deserialize<List<TrackTimeDto>>(json)!,
            allowedQuadClasses: [QuadClasses.Micro]);

        times.Should().HaveCount(1);
        times[0].PlayerName.Should().Be("SWEEPER");
    }

    [Fact]
    public async Task empty_quad_class_filter_allows_all_models()
    {
        var json = /*language:json*/"""
                                    [
                                        {
                                          "lap_time": "56.055",
                                          "playername": "PILOT_A",
                                          "model_name": "Hornet",
                                          "country": "UA",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:00:00Z",
                                          "updated_at": "2026-01-01T10:00:00Z",
                                          "user_id": 1001
                                        },
                                        {
                                          "lap_time": "57.000",
                                          "playername": "PILOT_B",
                                          "model_name": "Mobula 6",
                                          "country": "UA",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:01:00Z",
                                          "updated_at": "2026-01-01T10:01:00Z",
                                          "user_id": 1002
                                        }
                                    ]
                                    """;

        var models = new[]
        {
            new QuadModel { Id = 1, Name = "Hornet", Class = QuadClasses.Race },
            new QuadModel { Id = 2, Name = "Mobula 6", Class = QuadClasses.Micro }
        };

        var converter = CreateConverter(quadModels: models);
        var times = await converter.ConvertTrackTimesAsync(
            JsonSerializer.Deserialize<List<TrackTimeDto>>(json)!,
            allowedQuadClasses: []);

        times.Should().HaveCount(2);
    }

    private static RaceResultsConverter CreateConverter(
        string[] whitelistedPilots = null!,
        string[] blacklistedCountries = null!,
        QuadModel[] quadModels = null!)
    {
        var options = Options.Create(new ResultsOptions
        {
            CountriesBlackList = blacklistedCountries?.ToList() ?? []
        });
        return new RaceResultsConverter(
            new FakeWhiteListService(whitelistedPilots ?? []),
            new FakeQuadModelRepository(quadModels ?? []),
            options);
    }

    private sealed class FakeWhiteListService : IWhiteListService
    {
        private readonly IReadOnlySet<string> _whitelistedPilots;

        public FakeWhiteListService(IEnumerable<string> whitelistedPilots)
        {
            _whitelistedPilots = new HashSet<string>(whitelistedPilots);
        }

        public Task AddToWhiteListAsync(string pilotName) => throw new NotImplementedException();

        public Task RemoveFromWhiteListAsync(string pilotName) => throw new NotImplementedException();

        public Task<IReadOnlySet<string>> GetWhitelistAsync()
            => Task.FromResult(_whitelistedPilots);
    }

    private sealed class FakeQuadModelRepository : IRepository<QuadModel>
    {
        private readonly List<QuadModel> _models;

        public FakeQuadModelRepository(IEnumerable<QuadModel> models)
        {
            _models = models.ToList();
        }

        public IQueryable<QuadModel> GetAll() => _models.AsQueryable();

        public IQueryable<QuadModel> GetAll(Expression<Func<QuadModel, bool>> predicate) => _models.AsQueryable().Where(predicate);

        public ValueTask<QuadModel?> FindAsync(object id) => throw new NotImplementedException();
        public Task AddAsync(QuadModel entry) => throw new NotImplementedException();
        public Task AddRangeAsync(IEnumerable<QuadModel> entries) => throw new NotImplementedException();
        public Task UpdateAsync(QuadModel entry) => throw new NotImplementedException();
        public Task RemoveAsync(object id) => throw new NotImplementedException();
        public Task SaveChangesAsync() => throw new NotImplementedException();
        public Task SaveChangesAsync(CancellationToken ct) => throw new NotImplementedException();
    }
}