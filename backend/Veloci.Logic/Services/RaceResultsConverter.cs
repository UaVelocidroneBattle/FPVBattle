using System.Globalization;
using Veloci.Data.Domain;
using Veloci.Logic.API.Dto;

namespace Veloci.Logic.Services;

public class RaceResultsConverter
{
    private static readonly Mappings.DtoMapper _mapper = new();
    private readonly WhiteListService _whiteListService;

    public RaceResultsConverter(WhiteListService whiteListService)
    {
        _whiteListService = whiteListService;
    }

    public async Task<List<TrackTime>> ConvertTrackTimesAsync(IEnumerable<TrackTimeDto> timesDtos)
    {
        var whitelist = await _whiteListService.GetWhitelistAsync();

        return timesDtos
            .Where(dto => IsAllowed(dto, whitelist))
            .Select(MapDtoToTrackTime)
            .GroupBy(x => x.UserId)
            .Select(j => j.MinBy(x => x.Time))
            .Select((x, i) =>
            {
                x.LocalRank = i + 1;
                return x;
            })
            .ToList();
    }

    private static bool IsAllowed(TrackTimeDto dto, IReadOnlySet<string> whitelist)
        => dto.country == "UA" || whitelist.Contains(dto.playername);

    private static TrackTime MapDtoToTrackTime(TrackTimeDto dto, int index)
    {
        var time = _mapper.MapTrackTime(dto);
        time.Time = int.Parse(dto.lap_time.Replace(".", ""), CultureInfo.InvariantCulture);
        time.GlobalRank = index + 1;
        return time;
    }
}