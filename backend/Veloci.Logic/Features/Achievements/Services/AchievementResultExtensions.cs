namespace Veloci.Logic.Features.Achievements.Services;

public static class AchievementResultExtensions
{
    /// <summary>
    /// Groups results by the cup they belong to. Results without a cup are global —
    /// they land in a single group keyed by <c>null</c> and are meant for every cup.
    /// </summary>
    public static IEnumerable<(string? CupId, AchievementCheckResults Results)> GroupByCup(
        this IEnumerable<AchievementCheckResult> results)
    {
        return results
            .GroupBy(result => string.IsNullOrWhiteSpace(result.CupId) ? null : result.CupId)
            .Select(group =>
            {
                var groupedResults = new AchievementCheckResults();
                groupedResults.AddRange(group);
                return (group.Key, groupedResults);
            });
    }
}
