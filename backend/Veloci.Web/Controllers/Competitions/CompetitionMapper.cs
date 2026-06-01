using Riok.Mapperly.Abstractions;
using Veloci.Data.Domain;

namespace Veloci.Web.Controllers.Competitions;

[Mapper]
public static partial class CompetitionMapper
{
    [MapProperty(nameof(@Competition.Track.Map.Name), nameof(CompetitionModel.MapName))]
    [MapProperty(nameof(@Competition.Track.Map.MapId), nameof(CompetitionModel.MapId))]
    [MapProperty(nameof(@Competition.Track.TrackId), nameof(CompetitionModel.TrackId))]
    [MapProperty(nameof(@Competition.QuadOfTheDay.Name), nameof(CompetitionModel.QuadOfTheDay))]
    public static partial CompetitionModel MapToModel(this Competition competition);

    public static partial IQueryable<CompetitionModel> ProjectToModel(this IQueryable<Competition> competitions);

}
