using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;

namespace Veloci.Logic.Features.Achievements.Collection.Leagues;

/// <param name="league">Raw league to match against <see cref="Pilot.GetCurrentLeague"/>; null for pilots without a league.</param>
/// <param name="leagueDisplayName">Name used in the achievement's name, title and description.</param>
public class LeagueRacePlacementAchievement(string cupId, string? league, string leagueDisplayName, int place)
    : IAchievementAfterCompetition
{
    private static readonly string[] Ordinals = ["First", "Second", "Third"];
    private static readonly string[] Titles = ["Track Winner", "Runner-Up", "Third Place"];

    public string Name => $"{Ordinals[place - 1]}In{leagueDisplayName}";
    public string Title => $"{leagueDisplayName} {Titles[place - 1]}";
    public string Description => $"{Ordinals[place - 1]} place in a race ({leagueDisplayName} league)";
    public string? CupId => cupId;

    public async Task<bool> CheckAsync(Pilot pilot, Competition competition)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        if (pilot.GetCurrentLeague(cupId, competition.StartedOn) != league)
        {
            return false;
        }

        return competition.IsPilotAtLocalRank(pilot, place);
    }
}
