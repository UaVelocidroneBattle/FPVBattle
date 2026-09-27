using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Features.Leagues.Models;

namespace Veloci.Logic.Features.Achievements.Collection;

public abstract class LeagueUpdateAchievementBase : IAchievementAfterLeagueUpdate
{
    public abstract string Name { get; }
    public abstract string Title { get; }
    public abstract string Description { get; }
    public string? CupId => null;

    public Task<bool> CheckAsync(LeagueUpdateModel leagueUpdate)
    {
        if (leagueUpdate.Pilot.HasAchievement(Name))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(IsTriggered(leagueUpdate));
    }

    protected abstract bool IsTriggered(LeagueUpdateModel leagueUpdate);
}

/// <summary>
/// Base for achievements that depend on how a pilot's league rank changed, ignoring
/// updates where the pilot wasn't (or isn't now) ranked in any league.
/// </summary>
public abstract class LeagueRankChangeAchievementBase(ICupService cupService) : LeagueUpdateAchievementBase
{
    protected sealed override bool IsTriggered(LeagueUpdateModel leagueUpdate)
    {
        if (leagueUpdate.OldLeague is null || leagueUpdate.NewLeague is null)
        {
            return false;
        }

        var leagues = cupService.GetCupOptions(leagueUpdate.CupId).Leagues;
        var oldOrder = leagues.GetLeagueOrder(leagueUpdate.OldLeague);
        var newOrder = leagues.GetLeagueOrder(leagueUpdate.NewLeague);

        if (oldOrder is null || newOrder is null)
        {
            return false;
        }

        return IsRankChange(oldOrder.Value, newOrder.Value);
    }

    /// <summary>
    /// Order is ascending from the best league (0) to the worst.
    /// </summary>
    protected abstract bool IsRankChange(int oldOrder, int newOrder);
}
