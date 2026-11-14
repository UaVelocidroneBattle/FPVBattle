using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Persistence;

namespace FpvBattle.Application.Features.Achievements.Collection.OpenClass;

public class Haunter_OpenClass_Achievement(IApplicationDbContext db) : HaunterAchievementBase(db)
{
    public override string Name => "HaunterOpenClass";
    public override string Title => "Haunter (open class)";
    public override string Description => "Didn't miss a day in a season (open class)";
    public override string? CupId => CupIds.OpenClass;
}
