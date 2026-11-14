using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Persistence;

namespace FpvBattle.Application.Features.Achievements.Collection.WhoopClass;

public class Haunter_Whoop_Achievement(IApplicationDbContext db) : HaunterAchievementBase(db)
{
    public override string Name => "HaunterWhoopClass";
    public override string Title => "Haunter (whoop class)";
    public override string Description => "Didn't miss a day in a season (whoop class)";
    public override string? CupId => CupIds.WhoopClass;
}
