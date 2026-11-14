using Veloci.Logic.Features.Leagues.Models;

namespace Veloci.Logic.Features.Achievements.Collection;

public class DroppedAchievement : LeagueUpdateAchievementBase
{
    public override string Name => "Dropped";
    public override string Title => "Dropped";
    public override string Description => "Dropped off from the league";

    protected override bool IsTriggered(LeagueUpdateModel leagueUpdate) =>
        leagueUpdate.OldLeague is not null && leagueUpdate.NewLeague is null;
}
