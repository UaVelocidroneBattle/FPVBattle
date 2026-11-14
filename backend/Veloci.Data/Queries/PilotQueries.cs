using Microsoft.EntityFrameworkCore;
using Veloci.Domain;

namespace Veloci.Data.Queries;

public static class PilotQueries
{
    extension(IQueryable<Pilot> allPilots)
    {
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
