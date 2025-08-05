using Riok.Mapperly.Abstractions;
using Veloci.Data.Domain;
using Veloci.Logic.API.Dto;

namespace Veloci.Logic.Services;

[Mapper]
public partial class DtoMapper
{
    [MapProperty(nameof(TrackTimeDto.playername), nameof(TrackTime.PlayerName))]
    [MapProperty(nameof(TrackTimeDto.model_name), nameof(TrackTime.ModelName))]
    [MapProperty(nameof(TrackTimeDto.user_id), nameof(TrackTime.UserId))]
    [MapProperty(nameof(TrackTimeDto.updated_at), nameof(TrackTime.UpdatedAt))]
    public partial TrackTime MapTrackTime(TrackTimeDto timesDtos);

    private DateTime ConvertStringToDateTime(string stringDateTime)
        => DateTime.Parse(stringDateTime);
}
