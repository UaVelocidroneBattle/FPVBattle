using System.Text.Json;
using FluentAssertions;
using FpvBattle.Application.API.Velocidrone.Dto;
using FpvBattle.Application.Services;
using FpvBattle.Domain;
using FpvBattle.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FpvBattle.Tests;

public class ResponseConverterTests : IDisposable
{
    private readonly List<IDisposable> _databases = [];
    private readonly RaceResultsConverter _converter;

    public ResponseConverterTests()
    {
        _converter = CreateConverter();
    }

    public void Dispose()
    {
        _databases.ForEach(database => database.Dispose());
    }

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
        third.GlobalRank.Should().Be(4); // Sarah (1003, unregistered) was #3 globally; FPV FPV retains original API rank
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
    public async Task blacklisted_country_is_excluded_even_for_registered_pilot()
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

        var converter = CreateConverter(blacklistedCountries: ["RU"]);
        var times = await converter.ConvertTrackTimesAsync(data!, []);

        times.Should().HaveCount(1);
        times[0].PlayerName.Should().Be("SWEEPER");
    }

    [Fact]
    public async Task pilot_with_active_claim_is_included_before_pilot_exists()
    {
        var json = /*language:json*/"""
                                    [
                                        {
                                          "lap_time": "56.055",
                                          "playername": "NEWCOMER",
                                          "model_name": "5inch",
                                          "country": "NL",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:00:00Z",
                                          "updated_at": "2026-01-01T10:00:00Z",
                                          "user_id": 9999
                                        }
                                    ]
                                    """;
        var data = JsonSerializer.Deserialize<List<TrackTimeDto>>(json);
        data.Should().NotBeNull();

        var converter = CreateConverter(claims: [ActiveClaim("NEWCOMER")]);
        var times = await converter.ConvertTrackTimesAsync(data!, []);

        times.Should().HaveCount(1);
        times[0].PlayerName.Should().Be("NEWCOMER");
    }

    [Fact]
    public async Task claimed_pilot_name_match_is_case_sensitive()
    {
        var json = /*language:json*/"""
                                    [
                                        {
                                          "lap_time": "56.055",
                                          "playername": "NEWCOMER",
                                          "model_name": "5inch",
                                          "country": "NL",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:00:00Z",
                                          "updated_at": "2026-01-01T10:00:00Z",
                                          "user_id": 9999
                                        }
                                    ]
                                    """;
        var data = JsonSerializer.Deserialize<List<TrackTimeDto>>(json);
        data.Should().NotBeNull();

        var converter = CreateConverter(claims: [ActiveClaim("newcomer")]);
        var times = await converter.ConvertTrackTimesAsync(data!, []);

        times.Should().BeEmpty();
    }

    [Fact]
    public async Task expired_claim_is_ignored()
    {
        var json = /*language:json*/"""
                                    [
                                        {
                                          "lap_time": "56.055",
                                          "playername": "NEWCOMER",
                                          "model_name": "5inch",
                                          "country": "NL",
                                          "sim_version": "1.0.0",
                                          "device_type": 1,
                                          "created_at": "2026-01-01T10:00:00Z",
                                          "updated_at": "2026-01-01T10:00:00Z",
                                          "user_id": 9999
                                        }
                                    ]
                                    """;
        var data = JsonSerializer.Deserialize<List<TrackTimeDto>>(json);
        data.Should().NotBeNull();

        var converter = CreateConverter(claims: [ExpiredClaim("NEWCOMER")]);
        var times = await converter.ConvertTrackTimesAsync(data!, []);

        times.Should().BeEmpty();
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

    private RaceResultsConverter CreateConverter(
        int[] pilotIds = null!,
        PilotClaim[] claims = null!,
        string[] blacklistedCountries = null!,
        QuadModel[] quadModels = null!)
    {
        var options = Options.Create(new ResultsOptions
        {
            CountriesBlackList = blacklistedCountries?.ToList() ?? []
        });

        var db = CreateDatabase();

        // Sarah (1003) is intentionally not registered — tests rely on unknown pilots being filtered out
        db.Set<Pilot>().AddRange((pilotIds ?? [1001, 1002, 1004]).Select(id => new Pilot($"pilot_{id}") { Id = id, Country = "UA" }));
        db.Set<QuadModel>().AddRange(quadModels ?? []);

        foreach (var (claim, index) in (claims ?? []).Select((claim, index) => (claim, index)))
        {
            var claimant = new ApplicationUser { Id = $"claimant_{index}", UserName = $"claimant_{index}" };
            db.Users.Add(claimant);
            claim.UserId = claimant.Id;
            db.Set<PilotClaim>().Add(claim);
        }

        db.SaveChanges();
        return new RaceResultsConverter(db, options);
    }

    private ApplicationDbContext CreateDatabase()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseLazyLoadingProxies()
            .UseSqlite(connection)
            .Options);
        db.Database.EnsureCreated();

        _databases.Add(db);
        _databases.Add(connection);
        return db;
    }

    private static PilotClaim ActiveClaim(string pilotName) => new()
    {
        PilotName = pilotName,
        ExpiresOn = DateTime.UtcNow.AddHours(1)
    };

    private static PilotClaim ExpiredClaim(string pilotName) => new()
    {
        PilotName = pilotName,
        ExpiresOn = DateTime.UtcNow.AddHours(-1)
    };
}
