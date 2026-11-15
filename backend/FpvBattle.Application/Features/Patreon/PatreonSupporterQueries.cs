using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Patreon;

public static class PatreonSupporterQueries
{
    public static IQueryable<PatreonSupporter> Paying(this IQueryable<PatreonSupporter> query)
    {
        return query.Where(s => s.Status == PatreonSupporter.ActivePatronStatus && s.Amount > 0);
    }
}
