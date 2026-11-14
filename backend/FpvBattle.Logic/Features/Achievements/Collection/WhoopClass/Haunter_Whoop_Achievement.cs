using FpvBattle.Data.Repositories;
using FpvBattle.Domain;
using FpvBattle.Logic.Features.Cups;

namespace FpvBattle.Logic.Features.Achievements.Collection.WhoopClass;

public class Haunter_Whoop_Achievement(IRepository<Competition> competitions) : HaunterAchievementBase(competitions)
{
    public override string Name => "HaunterWhoopClass";
    public override string Title => "Haunter (whoop class)";
    public override string Description => "Didn't miss a day in a season (whoop class)";
    public override string? CupId => CupIds.WhoopClass;
}
