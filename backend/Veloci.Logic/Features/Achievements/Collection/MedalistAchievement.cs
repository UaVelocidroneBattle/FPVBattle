using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;

namespace Veloci.Logic.Features.Achievements.Collection;

public class MedalistAchievement : IAchievementAfterSeason
{
    public string Name => "Medalist";
    public string Title => "Медаліст";
    public string Description => "Пілот, який зібрав найбільшу кількість медалей за сезон";
    public async Task<bool> CheckAsync(Pilot pilot, List<SeasonResult> seasonResults)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        var medalist = GetMedalist(seasonResults);
        return medalist == pilot.Name;
    }

    /// <summary>
    /// Determines the pilot with the biggest medal amount.
    /// If amount is the same, compares gold medals, then silver, then bronze.
    /// If there is still a tie, returns null.
    /// </summary>
    private string? GetMedalist(List<SeasonResult> seasonResults)
    {
        var topPilots = seasonResults
            .Select(sr => new
            {
                Pilot = sr.PlayerName,
                Gold = sr.GoldenCount,
                Silver = sr.SilverCount,
                Bronze = sr.BronzeCount,
                Total = sr.GoldenCount + sr.SilverCount + sr.BronzeCount
            })
            .OrderByDescending(x => x.Total)
            .ThenByDescending(x => x.Gold)
            .ThenByDescending(x => x.Silver)
            .ToList();

        var winner = topPilots.First();
        var isTie = topPilots
            .Skip(1)
            .Any(p =>
                p.Total == winner.Total &&
                p.Gold == winner.Gold &&
                p.Silver == winner.Silver &&
                p.Bronze == winner.Bronze);

        return isTie ? null : winner.Pilot;
    }
}
