using System.Text.RegularExpressions;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Services.Tracks.Models;

namespace Veloci.Logic.Services.Tracks;

/// <summary>
/// Filters tracks based on cup-specific configuration:
/// scene whitelist, track type whitelist/blacklist, and name blacklist patterns.
/// </summary>
public class TrackFilter
{
    private readonly IReadOnlyDictionary<int, string>? _whitelistScenes;
    private readonly HashSet<int>? _whitelistTrackTypes;
    private readonly HashSet<int>? _blacklistTrackTypes;
    private readonly Regex[]? _blacklistRegexes;

    public TrackFilter(TrackFilterOptions options)
    {
        if (options.WhitelistScenes.Count > 0)
            _whitelistScenes = options.WhitelistScenes;

        if (options.WhitelistTrackTypes.Count > 0)
            _whitelistTrackTypes = options.WhitelistTrackTypes.ToHashSet();

        if (options.BlacklistTrackTypes.Count > 0)
            _blacklistTrackTypes = options.BlacklistTrackTypes.ToHashSet();

        if (options.BlacklistPatterns.Count > 0)
        {
            _blacklistRegexes = options.BlacklistPatterns
                .Select(pattern => new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
                .ToArray();
        }
    }

    /// <summary>
    /// Filters maps to whitelisted scenes, maps scene names, and returns suitable tracks.
    /// </summary>
    public List<ParsedTrackModel> GetSuitableTracks(IEnumerable<ParsedMapModel> maps)
    {
        var filteredMaps = _whitelistScenes != null
            ? maps.Where(m => _whitelistScenes.ContainsKey(m.Id)).ToList()
            : maps.ToList();

        if (_whitelistScenes != null)
        {
            // A bit of weird solution since we do not get scene names from API
            // so we need to map scene names from our scene whitelist
            foreach (var map in filteredMaps)
                map.Name = _whitelistScenes[map.Id];
        }

        return filteredMaps
            .SelectMany(m => m.Tracks)
            .Where(IsTrackSuitable)
            .ToList();
    }

    /// <summary>
    /// Determines if a track passes track-level filters (type and name).
    /// </summary>
    public bool IsTrackSuitable(ParsedTrackModel track)
    {
        if (_whitelistTrackTypes != null && !_whitelistTrackTypes.Contains(track.Type))
            return false;

        if (_blacklistTrackTypes != null && _blacklistTrackTypes.Contains(track.Type))
            return false;

        if (_blacklistRegexes != null && _blacklistRegexes.Any(regex => regex.IsMatch(track.Name)))
            return false;

        return true;
    }
}
