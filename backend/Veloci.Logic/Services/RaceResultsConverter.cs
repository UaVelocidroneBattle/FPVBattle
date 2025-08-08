using System.Globalization;
using Veloci.Data.Domain;
using Veloci.Logic.API.Dto;

namespace Veloci.Logic.Services;

public class RaceResultsConverter
{
    private static readonly DtoMapper _mapper = new();

    public List<TrackTime> ConvertTrackTimes(IEnumerable<TrackTimeDto> timesDtos)
    {

        return timesDtos
            .Select(MapDtoToTrackTime)
            .Where(x => x != null)
            .GroupBy(x => x.UserId)
            .Select(j => j.MinBy(x => x.Time))
            .Select((x, i) =>
            {
                x.LocalRank = i + 1;
                return x;
            })
            .ToList();
    }

    private TrackTime? MapDtoToTrackTime(TrackTimeDto dto, int index)
    {
        if (dto.country != "UA") return null;

        var time = _mapper.MapTrackTime(dto);
        time.Time = int.Parse(dto.lap_time.Replace(".", ""), CultureInfo.InvariantCulture);
        time.GlobalRank = index + 1;
        return time;
    }
}
