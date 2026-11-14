using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;

namespace Veloci.Logic.Features.Achievements.Collection.Leagues;

public class LeagueSeasonPlacementAchievement(string cupId, string leagueDisplayName, int place)
    : IAchievementAfterSeason
{
    private static readonly string[] Ordinals = ["First", "Second", "Third"];
    private static readonly string[] Titles = ["Champion", "Season Runner-Up", "Season Third Place"];

    public string Name => $"{Ordinals[place - 1]}InSeasonIn{leagueDisplayName}";
    public string Title => $"{leagueDisplayName} {Titles[place - 1]}";
    public string Description => $"{Ordinals[place - 1]} place in season ({leagueDisplayName} league)";
    public string? CupId => cupId;

    public async Task<bool> CheckAsync(Pilot pilot, List<LeagueSeasonLeaderboard> seasonLeaderboards)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        var leaderboard = seasonLeaderboards.SingleOrDefault(l => l.League == leagueDisplayName);
        return leaderboard?.Results.GetByPlace(place)?.PlayerName == pilot.Name;
    }
}
