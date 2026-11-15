using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Leagues;

public static class PilotLeagueQueries
{
    extension(IQueryable<PilotLeague> allRecords)
    {
        public IQueryable<PilotLeague> ForCup(string cupId)
        {
            return allRecords.Where(p => p.CupId == cupId);
        }

        public IQueryable<PilotLeague> ForPilot(int pilotId)
        {
            return allRecords.Where(p => p.PilotId == pilotId);
        }

        public IQueryable<PilotLeague> Active()
        {
            return allRecords.Where(p => p.Status == LeagueRecordStatus.Current);
        }
    }
}
