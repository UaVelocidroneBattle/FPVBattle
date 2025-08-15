using Riok.Mapperly.Abstractions;
using Veloci.Data.Domain;

namespace Veloci.Web.Controllers.Competitions;

[Mapper]
public static partial class CompetitionMapper
{
    [MapProperty(nameof(@Competition.Track.Map.Name), nameof(CompetitionModel.MapName))]
    [MapProperty(nameof(@Competition.Track.Map.MapId), nameof(CompetitionModel.MapId))]
    [MapProperty(nameof(@Competition.Track.TrackId), nameof(CompetitionModel.TrackId))]
    public static partial CompetitionModel MapToModel(this Competition competition);

    public static partial IQueryable<CompetitionModel> ProjectToModel(this IQueryable<Competition> competitions);

    public static partial List<TrackTimeModel> MapToModel(this IEnumerable<TrackTime> results);

    public static partial List<SeasonResultModel> MapToModel(this IEnumerable<SeasonResult> results);

    public static partial List<TrackTimeModel> MapToModel(this IEnumerable<CompetitionResults> results);

    [MapProperty(nameof(CompetitionResults.TrackTime), nameof(TrackTimeModel.Time))]
    [MapProperty(nameof(CompetitionResults.Pilot.Name), nameof(TrackTimeModel.PlayerName))]
    public static partial TrackTimeModel MapToModel(this CompetitionResults results);
}
