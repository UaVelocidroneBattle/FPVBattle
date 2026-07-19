using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.API.Dto;

namespace Veloci.Logic.Services;

public class RaceResultsConverter
{
    private static readonly Mappings.DtoMapper Mapper = new();
    private readonly IRepository<QuadModel> _quadModels;
    private readonly IRepository<Pilot> _pilots;
    private readonly IRepository<PilotClaim> _claims;
    private readonly ResultsOptions _options;

    public RaceResultsConverter(
        IRepository<QuadModel> quadModels,
        IRepository<Pilot> pilots,
        IRepository<PilotClaim> claims,
        IOptions<ResultsOptions> options)
    {
        _quadModels = quadModels;
        _pilots = pilots;
        _claims = claims;
        _options = options.Value;
    }

    public async Task<List<TrackTime>> ConvertTrackTimesAsync(IEnumerable<TrackTimeDto> timesDtos, int[] allowedQuadClasses, QuadModel? quadOfTheDay = null)
    {
        var pilotIds = await _pilots.GetAll().Select(x => x.Id).ToHashSetAsync();
        var claimedPilotNames = await GetActiveClaimNamesAsync();
        var modelClassByName = _quadModels.GetAll()
            .ToDictionary(m => m.Name, m => m.Class, StringComparer.OrdinalIgnoreCase);

        return timesDtos
            .Select((dto, i) => (dto, globalRank: i + 1))
            .Where(x => IsAllowed(x.dto, pilotIds, claimedPilotNames, modelClassByName, allowedQuadClasses))
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

    /// <summary>
    /// Pilots with an active claim may not exist in the database yet ("fly-to-verify"),
    /// so their results are matched by name until the first race creates the pilot.
    /// </summary>
    private async Task<IReadOnlySet<string>> GetActiveClaimNamesAsync()
    {
        var now = DateTime.UtcNow;
        var names = await _claims.GetAll(c => c.ExpiresOn > now)
            .Select(c => c.PilotName)
            .ToListAsync();

        // Velocidrone pilot names are case sensitive ("Jack" and "jack" are different pilots)
        return names.ToHashSet(StringComparer.Ordinal);
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

    private bool IsAllowed(
        TrackTimeDto dto,
        IReadOnlySet<int> pilotIds,
        IReadOnlySet<string> claimedPilotNames,
        Dictionary<string, int> modelClassByName,
        int[] allowedQuadClasses)
    {
        if (_options.CountriesBlackList.Contains(dto.country, StringComparer.OrdinalIgnoreCase))
            return false;

        if (allowedQuadClasses.Length > 0
            && modelClassByName.TryGetValue(dto.model_name, out var modelClass)
            && !allowedQuadClasses.Contains(modelClass))
            return false;

        return pilotIds.Contains(dto.user_id) || claimedPilotNames.Contains(dto.playername);
    }

    private static TrackTime MapDtoToTrackTime(TrackTimeDto dto, int globalRank)
    {
        var time = Mapper.MapTrackTime(dto);
        time.Time = int.Parse(dto.lap_time.Replace(".", ""), CultureInfo.InvariantCulture);
        time.GlobalRank = globalRank;
        return time;
    }
}