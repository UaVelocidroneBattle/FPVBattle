using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veloci.Data;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Services;

public class DbMigrator
{
    private readonly IRepository<Pilot> _pilots;
    private readonly IRepository<TrackMap> _trackMaps;
    private readonly IRepository<Track> _tracks;
    private readonly IRepository<Competition> _competitions;
    private readonly ILogger<DbMigrator> _logger;

    public DbMigrator(
        ILogger<DbMigrator> logger,
        IRepository<Pilot> pilots,
        IRepository<TrackMap> trackMaps,
        IRepository<Track> tracks,
        IRepository<Competition> competitions)
    {
        _logger = logger;
        _pilots = pilots;
        _trackMaps = trackMaps;
        _tracks = tracks;
        _competitions = competitions;
    }

    public async Task MigrateAsync(ApplicationDbContext sourceDb)
    {
        _logger.LogInformation("Starting database migration");
        _logger.LogInformation("Applying pending migrations to source database");
        await sourceDb.Database.MigrateAsync();

        await MigratePilotsAsync(sourceDb);
        await MigrateMapsAndTracksAsync(sourceDb);
        await MigrateCompetitionsAsync(sourceDb);

        _logger.LogInformation("Database migration complete");
    }

    private async Task MigratePilotsAsync(ApplicationDbContext sourceDb)
    {
        var sourcePilots = await sourceDb
            .Set<Pilot>()
            .Include(x => x.Achievements)
            .Include(x => x.DayStreakFreezes)
            .AsNoTracking()
            .ToListAsync();

        _logger.LogInformation("Found {Count} pilots in source database", sourcePilots.Count);

        var created = 0;
        var updated = 0;

        foreach (var sourcePilot in sourcePilots)
        {
            var isNew = await MigratePilotAsync(sourcePilot);
            if (isNew) created++; else updated++;
        }

        _logger.LogInformation("Pilots migration complete: {Created} created, {Updated} updated", created, updated);
    }

    private async Task<bool> MigratePilotAsync(Pilot sourcePilot)
    {
        var pilot = await _pilots.FindAsync(sourcePilot.Id);
        var isNew = false;

        if (pilot is null)
        {
            isNew = true;

            _logger.LogInformation("Creating pilot {PilotName} (Id: {PilotId})", sourcePilot.Name, sourcePilot.Id);

            pilot = new Pilot
            {
                Id = sourcePilot.Id,
                Name = sourcePilot.Name,
                Achievements = new List<PilotAchievement>(),
                DayStreakFreezes = new List<DayStreakFreeze>()
            };

            await _pilots.AddAsync(pilot);
        }
        else
        {
            _logger.LogInformation("Updating pilot {PilotName} (Id: {PilotId})", sourcePilot.Name, sourcePilot.Id);
        }

        MigratePilotStats(sourcePilot, pilot);
        MigratePilotFreezies(sourcePilot, pilot);
        MigratePilotAchievements(sourcePilot, pilot);

        await _pilots.SaveChangesAsync();

        return isNew;
    }

    private static readonly Dictionary<string, string> WhoopAchievementsMap = new()
    {
        { "FirstInSeason",     "FirstInSeason_Whoop" },
        { "FirstPlaceInRace",  "FirstPlaceInRace_Whoop" },
        { "SecondInSeason",    "SecondInSeason_Whoop" },
        { "SecondPlaceInRace", "SecondPlaceInRace_Whoop" },
        { "ThirdInSeason",     "ThirdInSeason_Whoop" },
        { "ThirdPlaceInRace",  "ThirdPlaceInRace_Whoop" },
    };

    private static void MigratePilotStats(Pilot sourcePilot, Pilot pilot)
    {
        pilot.LastRaceDate = new[] { pilot.LastRaceDate, sourcePilot.LastRaceDate }.Max();
        pilot.DayStreak = Math.Max(pilot.DayStreak, sourcePilot.DayStreak);
        pilot.MaxDayStreak = Math.Max(pilot.MaxDayStreak, sourcePilot.MaxDayStreak);
    }

    private static void MigratePilotFreezies(Pilot sourcePilot, Pilot pilot)
    {
        foreach (var freezie in sourcePilot.DayStreakFreezes.Where(x => x.SpentOn is null))
            pilot.DayStreakFreezes.Add(new DayStreakFreeze(freezie.CreatedOn));
    }

    private static void MigratePilotAchievements(Pilot sourcePilot, Pilot pilot)
    {
        foreach (var achievement in sourcePilot.Achievements)
        {
            var name = WhoopAchievementsMap.GetValueOrDefault(achievement.Name) ?? achievement.Name;

            if (pilot.HasAchievement(name))
                continue;

            pilot.Achievements.Add(new PilotAchievement { Pilot = pilot, Date = achievement.Date, Name = name });
        }
    }

    private async Task MigrateMapsAndTracksAsync(ApplicationDbContext sourceDb)
    {
        await MigrateMapsAsync(sourceDb);
        await MigrateTracksAsync(sourceDb);
    }

    private async Task MigrateMapsAsync(ApplicationDbContext sourceDb)
    {
        _logger.LogInformation("Migrating maps from source database");

        var sourceMaps = await sourceDb.Set<TrackMap>()
            .AsNoTracking()
            .ToListAsync();

        foreach (var sourceMap in sourceMaps)
        {
            var exists = _trackMaps.GetAll().Any(x => x.MapId == sourceMap.MapId);

            if (exists)
                continue;

            _logger.LogInformation("Creating map {MapName} (Id: {MapId})", sourceMap.Name, sourceMap.MapId);

            await _trackMaps.AddAsync(new TrackMap
            {
                MapId = sourceMap.MapId,
                Name = sourceMap.Name,
            });
        }

        _logger.LogInformation("Maps migration complete");
    }

    private async Task MigrateTracksAsync(ApplicationDbContext sourceDb)
    {
        _logger.LogInformation("Migrating tracks from source database");

        var sourceTracks = await sourceDb.Set<Track>()
            .Include(x => x.Map)
            .Include(x => x.Rating)
            .AsNoTracking()
            .ToListAsync();

        foreach (var sourceTrack in sourceTracks)
            await MigrateTrackAsync(sourceTrack);

        _logger.LogInformation("Tracks migration complete");
    }

    private async Task MigrateTrackAsync(Track sourceTrack)
    {
        var targetMap = _trackMaps.GetAll().FirstOrDefault(x => x.MapId == sourceTrack.Map.MapId);

        if (targetMap is null)
        {
            _logger.LogWarning("Map with MapId {MapId} not found in target database, skipping track {TrackName}",
                sourceTrack.Map.MapId, sourceTrack.Name);
            return;
        }

        var track = await _tracks.GetAll()
            .Include(x => x.Rating)
            .FirstOrDefaultAsync(x => x.TrackId == sourceTrack.TrackId);

        if (track is null)
        {
            _logger.LogInformation("Creating track {TrackName} (Id: {TrackId})", sourceTrack.Name, sourceTrack.TrackId);

            await _tracks.AddAsync(new Track
            {
                TrackId = sourceTrack.TrackId,
                Name = sourceTrack.Name,
                MapId = targetMap.Id,
                Rating = sourceTrack.Rating is null ? null : new TrackRating
                {
                    PollMessageId = sourceTrack.Rating.PollMessageId,
                    Value = sourceTrack.Rating.Value,
                }
            });

            return;
        }

        if (sourceTrack.Rating is not null && track.Rating is null)
        {
            _logger.LogInformation("Adding rating to existing track {TrackName} (Id: {TrackId})", sourceTrack.Name, sourceTrack.TrackId);

            track.Rating = new TrackRating
            {
                PollMessageId = sourceTrack.Rating.PollMessageId,
                Value = sourceTrack.Rating.Value,
            };

            await _tracks.SaveChangesAsync();
        }
    }

    private async Task MigrateCompetitionsAsync(ApplicationDbContext sourceDb)
    {
        _logger.LogInformation("Migrating competitions from source database");

        var sourceCompetitions = await sourceDb.Set<Competition>()
            .Include(x => x.Track)
            .Include(x => x.InitialResults).ThenInclude(x => x.Times)
            .Include(x => x.CurrentResults).ThenInclude(x => x.Times)
            .Include(x => x.CompetitionResults)
            .Include(x => x.TimeDeltas)
            .Include(x => x.Variables)
            .AsNoTracking()
            .ToListAsync();

        foreach (var sourceCompetition in sourceCompetitions)
            await MigrateCompetitionAsync(sourceCompetition);

        _logger.LogInformation("Competitions migration complete");
    }

    private async Task MigrateCompetitionAsync(Competition sourceCompetition)
    {
        var exists = _competitions.GetAll().Any(x => x.Id == sourceCompetition.Id);

        if (exists)
            return;

        var targetTrack = _tracks.GetAll().FirstOrDefault(x => x.TrackId == sourceCompetition.Track.TrackId);

        if (targetTrack is null)
        {
            _logger.LogWarning("Track with TrackId {TrackId} not found, skipping competition {CompetitionId}",
                sourceCompetition.Track.TrackId, sourceCompetition.Id);
            return;
        }

        _logger.LogInformation("Creating competition {CompetitionId} on track {TrackName} ({Date:yyyy-MM-dd})",
            sourceCompetition.Id, sourceCompetition.Track.Name, sourceCompetition.StartedOn);

        await _competitions.AddAsync(new Competition
        {
            Id = sourceCompetition.Id,
            StartedOn = sourceCompetition.StartedOn,
            State = sourceCompetition.State,
            CupId = CupIds.WhoopClass,
            TrackId = targetTrack.Id,
            ResultsPosted = sourceCompetition.ResultsPosted,
            InitialResults = MapTrackResults(sourceCompetition.InitialResults, sourceCompetition.State),
            CurrentResults = MapTrackResults(sourceCompetition.CurrentResults, sourceCompetition.State),
            CompetitionResults = sourceCompetition.CompetitionResults.Select(r => new CompetitionResults
            {
                Id = r.Id,
                PilotId = r.PilotId,
                TrackTime = r.TrackTime,
                LocalRank = r.LocalRank,
                GlobalRank = r.GlobalRank,
                Points = r.Points,
                ModelName = r.ModelName,
            }).ToList(),
            TimeDeltas = sourceCompetition.TimeDeltas.Select(d => new TrackTimeDelta
            {
                Id = d.Id,
                PilotId = d.PilotId,
                TrackTime = d.TrackTime,
                TimeChange = d.TimeChange,
                Rank = d.Rank,
                RankOld = d.RankOld,
                LocalRank = d.LocalRank,
                LocalRankOld = d.LocalRankOld,
                ModelName = d.ModelName,
                Date = d.Date,
            }).ToList(),
            Variables = sourceCompetition.Variables.Select(v => new CompetitionVariable
            {
                Id = v.Id,
                Name = v.Name,
                StringValue = v.StringValue,
                IntValue = v.IntValue,
                ULongValue = v.ULongValue,
                DoubleValue = v.DoubleValue,
                BoolValue = v.BoolValue,
            }).ToList(),
        });
    }

    private static TrackResults MapTrackResults(TrackResults source, CompetitionState state)
    {
        if (state != CompetitionState.Started)
            return new TrackResults();

        return new TrackResults
        {
            Times = source.Times.Select(t => new TrackTime
            {
                Id = t.Id,
                Time = t.Time,
                PlayerName = t.PlayerName,
                UserId = t.UserId,
                ModelName = t.ModelName,
                GlobalRank = t.GlobalRank,
                LocalRank = t.LocalRank,
                UpdatedAt = t.UpdatedAt,
            }).ToList()
        };
    }
}
