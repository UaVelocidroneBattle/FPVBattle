using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Leagues.Services;

public class LeagueService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<LeagueService>();
    private readonly ICupService _cupService;
    private readonly RatingService _ratingService;
    private readonly IRepository<PilotLeague> _pilotLeagues;
    private readonly PaceRatingCalculator _paceRatingCalculator;

    public LeagueService(
        ICupService cupService,
        RatingService ratingService,
        IRepository<PilotLeague> pilotLeagues,
        PaceRatingCalculator paceRatingCalculator)
    {
        _cupService = cupService;
        _ratingService = ratingService;
        _pilotLeagues = pilotLeagues;
        _paceRatingCalculator = paceRatingCalculator;
    }

    public async Task UpdatePilotLeaguesAsync()
    {
        var cupIds = _cupService.GetEnabledCupIds();

        foreach (var cupId in cupIds)
        {
            var cupOptions = _cupService.GetCupOptions(cupId);

            if (!cupOptions.Leagues.Enabled)
            {
                continue;
            }

            await _paceRatingCalculator.CalculateForCupAsync(cupId);
            await UpdatePilotLeaguesAsync(cupId);
        }
    }

    private async Task UpdatePilotLeaguesAsync(string cupId)
    {
        Log.Information("Updating pilot leagues for cup {CupId}", cupId);

        var leagueDistribution = await GetLeagueDistributionAsync(cupId);

        foreach (var (pilotId, league) in leagueDistribution)
        {
            var pilotLeagueRecord = await _pilotLeagues
                .GetAll()
                .ForCup(cupId)
                .ForPilot(pilotId)
                .Active()
                .FirstOrDefaultAsync();

            if (pilotLeagueRecord is not null && pilotLeagueRecord.League == league)
                continue;

            Log.Information("Pilot {PilotId} moved to league {League}", pilotId, league);

            pilotLeagueRecord?.Status = LeagueRecordStatus.Historical;

            await _pilotLeagues.AddAsync(new PilotLeague
            {
                CupId = cupId,
                PilotId = pilotId,
                Date = DateTime.UtcNow,
                League = league,
                Status = LeagueRecordStatus.Current
            });
        }

        var distributedPilotIds = leagueDistribution.Select(x => x.PilotId).ToHashSet();

        var recordsToRetire = await _pilotLeagues
            .GetAll()
            .ForCup(cupId)
            .Active()
            .Where(r => !distributedPilotIds.Contains(r.PilotId))
            .ToListAsync();

        foreach (var record in recordsToRetire)
        {
            Log.Information("Pilot {PilotId} retired from league {League}", record.PilotId, record.League);
            record.Status = LeagueRecordStatus.Historical;
        }

        await _pilotLeagues.SaveChangesAsync();

        Log.Information("Pilot leagues updated for cup {CupId}", cupId);
    }

    private async Task<List<(int PilotId, string League)>> GetLeagueDistributionAsync(string cupId)
    {
        var cupOptions = _cupService.GetCupOptions(cupId);
        var pilotIds = await _ratingService.GetRankedPilotIdsAsync(cupId);
        var leagues = cupOptions.Leagues.Definitions.OrderBy(l => l.Order).ToList();
        var distribution = new List<(int PilotId, string League)>();
        var position = 0;

        foreach (var league in leagues)
        {
            var slice = league.Size == 0
                ? pilotIds.Skip(position)
                : pilotIds.Skip(position).Take(league.Size);

            foreach (var pilotId in slice)
                distribution.Add((pilotId, league.Name));

            position += league.Size;
        }

        return distribution;
    }
}
