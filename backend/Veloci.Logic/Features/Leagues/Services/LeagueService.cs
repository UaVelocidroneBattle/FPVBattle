using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Features.Leagues.Models;
using Veloci.Logic.Features.Leagues.Notifications;

namespace Veloci.Logic.Features.Leagues.Services;

public class LeagueService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<LeagueService>();
    private readonly ICupService _cupService;
    private readonly RatingService _ratingService;
    private readonly IRepository<PilotLeague> _pilotLeagues;
    private readonly PaceRatingCalculator _paceRatingCalculator;
    private readonly IMediator _mediator;

    public LeagueService(
        ICupService cupService,
        RatingService ratingService,
        IRepository<PilotLeague> pilotLeagues,
        PaceRatingCalculator paceRatingCalculator,
        IMediator mediator)
    {
        _cupService = cupService;
        _ratingService = ratingService;
        _pilotLeagues = pilotLeagues;
        _paceRatingCalculator = paceRatingCalculator;
        _mediator = mediator;
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

        var leagueUpdates = new List<LeagueUpdateModel>();
        var leagueDistribution = await GetLeagueDistributionAsync(cupId);

        foreach (var (pilotId, league, pilotName) in leagueDistribution)
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

            leagueUpdates.Add(new LeagueUpdateModel
            {
                OldLeague = pilotLeagueRecord?.League,
                NewLeague = league,
                PilotName = pilotName
            });
        }

        var distributedPilotIds = leagueDistribution.Select(x => x.PilotId).ToHashSet();

        var recordsToRetire = await _pilotLeagues
            .GetAll()
            .ForCup(cupId)
            .Active()
            .Where(r => !distributedPilotIds.Contains(r.PilotId))
            .Include(r => r.Pilot)
            .ToListAsync();

        foreach (var record in recordsToRetire)
        {
            Log.Information("Pilot {PilotId} retired from league {League}", record.PilotId, record.League);
            record.Status = LeagueRecordStatus.Historical;

            // Records a dated "no league" marker, mirroring how a league change works, so that
            // date-based lookups (e.g. leaderboards for past competitions) stop resolving to the
            // pilot's last league once they've been retired from all leagues.
            await _pilotLeagues.AddAsync(new PilotLeague
            {
                CupId = cupId,
                PilotId = record.PilotId,
                Date = DateTime.UtcNow,
                League = null,
                Status = LeagueRecordStatus.Current
            });

            leagueUpdates.Add(new LeagueUpdateModel
            {
                OldLeague = record.League,
                NewLeague = null,
                PilotName = record.Pilot.Name
            });
        }

        await _pilotLeagues.SaveChangesAsync();

        if (leagueUpdates.Count != 0)
        {
            await _mediator.Publish(new LeagueUpdateNotification(cupId, leagueUpdates));
        }

        Log.Information("Pilot leagues updated for cup {CupId}", cupId);
    }

    private async Task<List<(int PilotId, string League, string PilotName)>> GetLeagueDistributionAsync(string cupId)
    {
        var cupOptions = _cupService.GetCupOptions(cupId);
        var ratings = await _ratingService.GetRatingsForCupAsync(cupId);
        var leagues = cupOptions.Leagues.Definitions.OrderBy(l => l.Order).ToList();
        var distribution = new List<(int PilotId, string League, string PilotName)>();
        var position = 0;

        foreach (var league in leagues)
        {
            var slice = league.Size == 0
                ? ratings.Skip(position)
                : ratings.Skip(position).Take(league.Size);

            foreach (var rating in slice)
                distribution.Add((rating.PilotId, league.Name, rating.Pilot.Name));

            if (league.Size == 0) break;

            position += league.Size;
        }

        return distribution;
    }
}
