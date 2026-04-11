using System.Globalization;
using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.API.Dto;

namespace Veloci.Logic.Services;

public class RaceResultsConverter
{
    private static readonly Mappings.DtoMapper Mapper = new();
    private readonly IWhiteListService _whiteListService;
    private readonly IRepository<QuadModel> _quadModels;
    private readonly ResultsOptions _options;

    public RaceResultsConverter(IWhiteListService whiteListService, IRepository<QuadModel> quadModels, IOptions<ResultsOptions> options)
    {
        _whiteListService = whiteListService;
        _quadModels = quadModels;
        _options = options.Value;
    }

    public async Task<List<TrackTime>> ConvertTrackTimesAsync(IEnumerable<TrackTimeDto> timesDtos, int[] allowedQuadClasses, QuadModel? quadOfTheDay = null)
    {
        var whitelist = await _whiteListService.GetWhitelistAsync();
        var modelClassByName = _quadModels.GetAll()
            .ToDictionary(m => m.Name, m => m.Class, StringComparer.OrdinalIgnoreCase);

        return timesDtos
            .Select((dto, i) => (dto, globalRank: i + 1))
            .Where(x => IsAllowed(x.dto, whitelist, modelClassByName, allowedQuadClasses))
            .Select(x => MapDtoToTrackTime(x.dto, x.globalRank))
            .GroupBy(x => x.UserId)
            .Select(group => SelectBestTime(group, quadOfTheDay))
            .OrderBy(x => x.Time)
            .Select((x, i) =>
            {
                x.LocalRank = i + 1;
                return x;
            })
            .ToList();
    }

    private static TrackTime SelectBestTime(IGrouping<int?, TrackTime> userGroup, QuadModel? quadOfTheDay)
    {
        var fastest = userGroup.MinBy(x => x.Time)!;

        if (quadOfTheDay is null)
            return fastest;

        return userGroup
            .Where(x => x.ModelName.Equals(quadOfTheDay.Name, StringComparison.OrdinalIgnoreCase))
            .MinBy(x => x.Time) ?? fastest;
    }

    private bool IsAllowed(TrackTimeDto dto, IReadOnlySet<string> whitelist, Dictionary<string, int> modelClassByName, int[] allowedQuadClasses)
    {
        if (_options.CountriesBlackList.Contains(dto.country, StringComparer.OrdinalIgnoreCase))
            return false;

        if (allowedQuadClasses.Length > 0
            && modelClassByName.TryGetValue(dto.model_name, out var modelClass)
            && !allowedQuadClasses.Contains(modelClass))
            return false;

        return dto.country == "UA" || whitelist.Contains(dto.playername);
    }

    private static TrackTime MapDtoToTrackTime(TrackTimeDto dto, int globalRank)
    {
        var time = Mapper.MapTrackTime(dto);
        time.Time = int.Parse(dto.lap_time.Replace(".", ""), CultureInfo.InvariantCulture);
        time.GlobalRank = globalRank;
        return time;
    }
}
