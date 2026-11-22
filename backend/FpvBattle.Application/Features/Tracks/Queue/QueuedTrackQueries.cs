using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Tracks.Queue;

public static class QueuedTrackQueries
{
    extension(IQueryable<QueuedTrack> query)
    {
        public IQueryable<QueuedTrack> ForCup(string cupId)
        {
            return query.Where(x => x.CupId == cupId);
        }

        public IQueryable<QueuedTrack> NotUsed()
        {
            return query.Where(x => !x.Used);
        }

        public IQueryable<QueuedTrack> Undated()
        {
            return query.Where(x => x.ScheduledOn == null);
        }

        public IQueryable<QueuedTrack> ScheduledBy(string userId)
        {
            return query.Where(x => x.ScheduledByUserId == userId);
        }

        public IQueryable<QueuedTrack> AddedSince(DateTime since)
        {
            return query.Where(x => x.AddedOn >= since);
        }
    }
}
