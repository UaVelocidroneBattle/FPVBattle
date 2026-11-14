using Riok.Mapperly.Abstractions;
using FpvBattle.Application.API.Velocidrone.Dto;
using FpvBattle.Domain;

namespace FpvBattle.Application.Mappings;

[Mapper]
public partial class DtoMapper
{
    [MapProperty(nameof(TrackTimeDto.playername), nameof(TrackTime.PlayerName))]
    [MapProperty(nameof(TrackTimeDto.model_name), nameof(TrackTime.ModelName))]
    [MapProperty(nameof(TrackTimeDto.user_id), nameof(TrackTime.UserId))]
    [MapProperty(nameof(TrackTimeDto.updated_at), nameof(TrackTime.UpdatedAt))]
    [MapProperty(nameof(TrackTimeDto.country), nameof(TrackTime.Country))]
    public partial TrackTime MapTrackTime(TrackTimeDto timesDtos);

    private DateTime ConvertStringToDateTime(string stringDateTime)
        => DateTime.Parse(stringDateTime);
}
