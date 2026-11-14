using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Achievements.Collection.OpenClass;

public class Haunter_OpenClass_Achievement(IRepository<Competition> competitions) : HaunterAchievementBase(competitions)
{
    public override string Name => "HaunterOpenClass";
    public override string Title => "Haunter (open class)";
    public override string Description => "Didn't miss a day in a season (open class)";
    public override string? CupId => CupIds.OpenClass;
}
