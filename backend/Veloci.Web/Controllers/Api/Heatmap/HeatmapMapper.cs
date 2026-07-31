using Riok.Mapperly.Abstractions;
using Veloci.Data.Domain;

namespace Veloci.Web.Controllers.Api.Heatmap;


[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class HeatmapMapper
{
    [MapProperty(nameof(@CompetitionResults.Competition.StartedOn), nameof(PilotResult.Date))]
    public static partial PilotResult ProjectToModel(this CompetitionResults results);

    public static partial IQueryable<PilotResult> ProjectToModel(this IQueryable<CompetitionResults> results);
}
