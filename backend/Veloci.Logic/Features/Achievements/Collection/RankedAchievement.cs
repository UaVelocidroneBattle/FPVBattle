using Veloci.Logic.Features.Leagues.Models;

namespace Veloci.Logic.Features.Achievements.Collection;

public class RankedAchievement : LeagueUpdateAchievementBase
{
    public override string Name => "Ranked";
    public override string Title => "Ranked";
    public override string Description => "Ranked into a league for the first time";

    protected override bool IsTriggered(LeagueUpdateModel leagueUpdate) =>
        leagueUpdate.OldLeague is null && leagueUpdate.NewLeague is not null;
}
