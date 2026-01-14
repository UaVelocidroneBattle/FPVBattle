using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Settings;

namespace Veloci.Logic.Services.Leagues;

public class LeagueQualifier
{
    private static readonly ILogger Log = Serilog.Log.ForContext<LeagueQualifier>();
    private readonly CompetitionService _competitionService;
    private readonly List<LeagueSettings> _leaguesSettings;
    private readonly IRepository<Pilot> _pilotRepository;
    private readonly IRepository<League> _leagueRepository;

    public LeagueQualifier(
        CompetitionService competitionService,
        IOptions<List<LeagueSettings>> options,
        IRepository<Pilot> pilotRepository,
        IRepository<League> leagueRepository)
    {
        _competitionService = competitionService;
        _pilotRepository = pilotRepository;
        _leagueRepository = leagueRepository;
        _leaguesSettings = options.Value;
    }

    // Initial pilots qualification
    public async Task QualifyPilotsAsync()
    {
        Log.Information("Starting initial pilot qualification");

        var needInitialQualification = _pilotRepository
            .GetAll()
            .All(pilot => pilot.League == null);

        if (!needInitialQualification)
        {
            Log.Information("Initial qualification not needed - pilots already have leagues assigned");
            return;
        }

        var today = DateTime.Today;
        var firstDayOfPreviousMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
        var firstDayOfCurrentMonth = new DateTime(today.Year, today.Month, 1);

        Log.Information("Fetching season results from {StartDate} to {EndDate}", firstDayOfPreviousMonth, firstDayOfCurrentMonth);
        var results = await _competitionService.GetSeasonResultsAsync(firstDayOfPreviousMonth, firstDayOfCurrentMonth);

        if (results.Count == 0)
        {
            Log.Warning("No season results found for qualification period");
            return;
        }

        Log.Information("Found {ResultCount} season results. Starting league assignment", results.Count);
        var skip = 0;

        foreach (var leagueSetting in _leaguesSettings)
        {
            var league = _leagueRepository.GetAll().FindByOrder(leagueSetting.Order);

            if (league is null)
            {
                Log.Error("League with order {LeagueOrder} not found in database", leagueSetting.Order);
                throw new Exception($"Can not find a league with order {leagueSetting.Order}. Something wrong with league configuration in the DB");
            }

            Log.Information("Processing league '{LeagueName}'", league.Name);

            var pilotNames = results
                .Skip(skip)
                .Take(leagueSetting.Size)
                .Select(r => r.PlayerName)
                .ToList();

            foreach (var pilotName in pilotNames)
            {
                var pilot = await _pilotRepository.GetAll().ByName(pilotName).FirstOrDefaultAsync();

                if (pilot is null)
                {
                    Log.Error("Pilot '{PilotName}' not found in database", pilotName);
                    throw new Exception($"Can not find a pilot with name {pilotName}");
                }

                pilot.League = league;
                pilot.LeagueHistory.Add(new LeagueHistoryRecord
                {
                    Date = today,
                    NewLeague = league,
                });

                Log.Debug("Assigned pilot '{PilotName}' to league '{LeagueName}'", pilotName, league.Name);
            }

            Log.Information("Completed league '{LeagueName}' - assigned {PilotCount} pilots", league.Name, pilotNames.Count);
            skip += leagueSetting.Size;
        }

        await _pilotRepository.SaveChangesAsync();
        Log.Information("Initial pilot qualification completed successfully");
    }
}
