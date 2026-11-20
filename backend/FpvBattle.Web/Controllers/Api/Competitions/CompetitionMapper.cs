using Riok.Mapperly.Abstractions;
using FpvBattle.Domain;

namespace FpvBattle.Web.Controllers.Api.Competitions;

[Mapper]
public static partial class CompetitionMapper
{
    [MapProperty(nameof(@Competition.Track.Scene.SceneId), nameof(CompetitionModel.SceneId))]
    [MapProperty(nameof(@Competition.Track.Scene.Name), nameof(CompetitionModel.SceneName))]
    [MapProperty(nameof(@Competition.Track.TrackId), nameof(CompetitionModel.TrackId))]
    [MapProperty(nameof(@Competition.QuadOfTheDay.Name), nameof(CompetitionModel.QuadOfTheDay))]
    public static partial CompetitionModel MapToModel(this Competition competition);

    public static partial IQueryable<CompetitionModel> ProjectToModel(this IQueryable<Competition> competitions);

}
