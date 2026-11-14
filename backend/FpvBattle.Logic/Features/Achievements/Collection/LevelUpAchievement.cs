using FpvBattle.Logic.Features.Cups;

namespace FpvBattle.Logic.Features.Achievements.Collection;

public class LevelUpAchievement(ICupService cupService) : LeagueRankChangeAchievementBase(cupService)
{
    public override string Name => "LevelUp";
    public override string Title => "Level Up";
    public override string Description => "Promoted to the next league";

    protected override bool IsRankChange(int oldOrder, int newOrder) => newOrder < oldOrder;
}
