using System.Globalization;
using Veloci.Data.Domain;
using Veloci.Logic.API.Dto;

namespace Veloci.Logic.Services;

public class RaceResultsConverter
{
    private static readonly Mappings.DtoMapper _mapper = new();
    private readonly IWhiteListService _whiteListService;

    public RaceResultsConverter(IWhiteListService whiteListService)
    {
        _whiteListService = whiteListService;
    }

    public async Task<List<TrackTime>> ConvertTrackTimesAsync(IEnumerable<TrackTimeDto> timesDtos)
    {
        var whitelist = await _whiteListService.GetWhitelistAsync();

        return timesDtos
            .Select((dto, i) => (dto, globalRank: i + 1))
            .Where(x => IsAllowed(x.dto, whitelist))
            .Select(x => MapDtoToTrackTime(x.dto, x.globalRank))
            .GroupBy(x => x.UserId)
            .Select(j => j.MinBy(x => x.Time)!)
            .Select((x, i) =>
            {
                x.LocalRank = i + 1;
                return x;
            })
            .ToList();
    }

    private static bool IsAllowed(TrackTimeDto dto, IReadOnlySet<string> whitelist)
        => dto.country == "UA" || whitelist.Contains(dto.playername);

    private static TrackTime MapDtoToTrackTime(TrackTimeDto dto, int globalRank)
    {
        var time = _mapper.MapTrackTime(dto);
        time.Time = int.Parse(dto.lap_time.Replace(".", ""), CultureInfo.InvariantCulture);
        time.GlobalRank = globalRank;
        return time;
    }
}