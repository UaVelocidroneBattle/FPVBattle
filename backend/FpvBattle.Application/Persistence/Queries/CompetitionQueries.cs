using FpvBattle.Application.Features.Tracks.Scheduling;
using FpvBattle.Application.Features.Tracks.Selection;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Application.Persistence.Queries;

public static class CompetitionQueries
{
    /// <param name="query">Source query</param>
    extension(IQueryable<Competition> query)
    {
        /// <summary>
        /// Filters competitions by date range
        /// </summary>
        /// <param name="from">From date inclusive</param>
        /// <param name="to">To date exclusive</param>
        /// <returns></returns>
        public IQueryable<Competition> InRange(DateTime from, DateTime to)
        {
            return query.Where(comp => comp.StartedOn >= from && comp.StartedOn < to);
        }

        /// <summary>
        /// Filters competitions that are not cancelled
        /// </summary>
        /// <returns></returns>
        public IQueryable<Competition> NotCancelled()
        {
            return query.Where(comp => comp.State != CompetitionState.Cancelled);
        }

        /// <summary>
        /// Filters competitions that have been closed, i.e. the ones whose final
        /// <see cref="Competition.CompetitionResults"/> have been calculated
        /// </summary>
        /// <returns></returns>
        public IQueryable<Competition> Closed()
        {
            return query.Where(comp => comp.State == CompetitionState.Closed);
        }

        public IQueryable<Competition> Active()
        {
            return query.Where(comp => comp.State == CompetitionState.Started);
        }

        public IQueryable<Competition> NewestFirst()
        {
            return query.OrderByDescending(comp => comp.StartedOn);
        }

        /// <summary>
        /// Filters competitions by cup ID
        /// </summary>
        /// <param name="cupId">Cup identifier (e.g., "open-class", "whoop")</param>
        /// <returns></returns>
        public IQueryable<Competition> ForCup(string cupId)
        {
            return query.Where(comp => comp.CupId == cupId);
        }

        /// <summary>
        /// Filters competitions where the specified pilot participated
        /// </summary>
        /// <param name="pilotId">Pilot identifier</param>
        /// <returns></returns>
        public IQueryable<Competition> ForPilot(int pilotId)
        {
            return query.Where(comp =>
                comp.CompetitionResults.Any(r => r.PilotId == pilotId));
        }

        /// <summary>
        /// Filters competitions by specific date (ignoring time component)
        /// </summary>
        /// <param name="date">Target date</param>
        /// <returns></returns>
        public IQueryable<Competition> OnDate(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return query.Where(comp =>
                comp.StartedOn >= startOfDay && comp.StartedOn < endOfDay);
        }

        /// <summary>
        /// The latest race of each given track that was raced. Cancelled competitions were replaced
        /// by a track change or a restart, so they don't count as raced.
        /// </summary>
        public async Task<Dictionary<string, LastRace>> GetLastRacesAsync(IReadOnlyCollection<string> trackIds)
        {
            var races = await query
                .NotCancelled()
                .Where(comp => trackIds.Contains(comp.TrackId))
                .Select(comp => new { comp.TrackId, comp.StartedOn, comp.CupId })
                .ToListAsync();

            return races
                .GroupBy(race => race.TrackId)
                .Select(group => group.MaxBy(race => race.StartedOn)!)
                .ToDictionary(
                    race => race.TrackId,
                    race => new LastRace(race.StartedOn, race.CupId, TrackReuseRules.IsInCooldown(race.StartedOn)));
        }
    }
}
