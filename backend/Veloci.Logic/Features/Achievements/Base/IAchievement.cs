using Veloci.Data.Domain;
using Veloci.Logic.Features.Leagues.Models;

namespace Veloci.Logic.Features.Achievements.Base;

public interface IAchievement
{
    string Name { get; }
    string Title { get; }
    string Description { get; }

    /// <summary>
    /// Returns cup id if the achievement is a cup-specific. Empty if achievement is not cup specific
    /// </summary>
    string? CupId { get; }
}

public interface IAchievementAfterTimeUpdate : IAchievement
{
    Task<bool> CheckAsync(Pilot pilot, List<TrackTimeDelta> deltas);
}

public interface IAchievementAfterCompetition : IAchievement
{
    Task<bool> CheckAsync(Pilot pilot, Competition competition);
}

public interface IAchievementAfterSeason : IAchievement
{
    Task<bool> CheckAsync(Pilot pilot, List<LeagueSeasonLeaderboard> seasonLeaderboards);
}

public interface IGlobalAchievement : IAchievement
{
    Task<Pilot?> CheckAsync();
}

public interface IAchievementAfterLeagueUpdate : IAchievement
{
    Task<bool> CheckAsync(LeagueUpdateModel leagueUpdate);
}
