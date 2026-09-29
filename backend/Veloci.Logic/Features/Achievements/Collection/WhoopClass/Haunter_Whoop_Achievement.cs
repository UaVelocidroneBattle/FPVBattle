using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Achievements.Collection.WhoopClass;

public class Haunter_Whoop_Achievement(IRepository<Competition> competitions) : HaunterAchievementBase(competitions)
{
    public override string Name => "HaunterWhoopClass";
    public override string Title => "Haunter (whoop class)";
    public override string Description => "Didn't miss a day in a season (whoop class)";
    public override string? CupId => CupIds.WhoopClass;
}
