namespace Veloci.Logic.Features.Achievements.Services;

public static class AchievementResultExtensions
{
    public static IEnumerable<(string CupId, AchievementCheckResults Results)> GroupByCup(
        this IEnumerable<AchievementCheckResult> results)
    {
        return results
            .Where(result => !string.IsNullOrWhiteSpace(result.CupId))
            .GroupBy(result => result.CupId!)
            .Select(group =>
            {
                var groupedResults = new AchievementCheckResults();
                groupedResults.AddRange(group);
                return (group.Key, groupedResults);
            });
    }
}
