using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Application.Persistence.Queries;

public static class PilotQueries
{
    extension(IQueryable<Pilot> allPilots)
    {
        public IQueryable<Pilot> ByName(string name)
        {
            return allPilots.Where(p => p.Name == name || p.NameHistory.Select(r => r.OldName).Contains(name));
        }

        public IQueryable<IGrouping<string, Pilot>> GroupedByCountry()
        {
            return allPilots.Where(p => !string.IsNullOrEmpty(p.Country)).GroupBy(p => p.Country);
        }

        public async Task ResetDayStreaksAsync(DateTime today)
        {
            await allPilots
                .Include(p => p.DayStreakFreezes)
                .Where(p => p.LastRaceDate < today && p.DayStreak > 0)
                .ForEachAsync(pilot => pilot.ResetDayStreak(today));
        }

        public async Task<Pilot?> FindByNameAsync(string name)
        {
            var normalized = name.Trim();
            var candidates = await allPilots.ByName(normalized).ToListAsync();
            return candidates.FirstOrDefault(p => string.Equals(p.Name, normalized, StringComparison.Ordinal));
        }
    }
}
