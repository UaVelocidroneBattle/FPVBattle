using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Tracks;

public static class TrackQueries
{
    public static IQueryable<Track> Available(this IQueryable<Track> query)
        => query.Where(t => !t.IsDeleted && !t.Scene.IsDeleted);
}
