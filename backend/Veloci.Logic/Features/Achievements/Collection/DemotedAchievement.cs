using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Achievements.Collection;

public class DemotedAchievement(ICupService cupService) : LeagueRankChangeAchievementBase(cupService)
{
    public override string Name => "Demoted";
    public override string Title => "Demoted";
    public override string Description => "Demoted to a lower league";

    protected override bool IsRankChange(int oldOrder, int newOrder) => newOrder > oldOrder;
}
