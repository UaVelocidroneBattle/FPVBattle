using System.Text.RegularExpressions;
using FpvBattle.Application.Features.Cups;
using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Tracks.Selection;

/// <summary>
/// Filters tracks by a cup's configuration: scene whitelist, track type whitelist/blacklist, and name blacklist patterns.
/// A track of unknown type passes only when the cup filters no types, since it cannot be shown to be of an allowed type.
/// </summary>
public class TrackFilter
{
    private readonly HashSet<int>? _whitelistSceneIds;
    private readonly HashSet<int>? _whitelistTrackTypes;
    private readonly HashSet<int>? _blacklistTrackTypes;
    private readonly Regex[]? _blacklistRegexes;

    public TrackFilter(TrackFilterOptions options)
    {
        if (options.WhitelistScenes.Count > 0)
            _whitelistSceneIds = options.WhitelistScenes.Keys.ToHashSet();

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

    public List<Track> GetSuitableTracks(IEnumerable<Track> tracks) => tracks.Where(IsTrackSuitable).ToList();

    /// <summary>
    /// The scene and type rules as a database query. Name patterns are regexes, so only <see cref="IsTrackSuitable"/> applies them.
    /// </summary>
    public IQueryable<Track> WhereSceneAndTypeAllowed(IQueryable<Track> tracks)
    {
        if (_whitelistSceneIds is { } sceneIds)
            tracks = tracks.Where(t => sceneIds.Contains(t.Scene.SceneId));

        if (_whitelistTrackTypes is { } allowedTypes)
            tracks = tracks.Where(t => t.Type != null && allowedTypes.Contains(t.Type.Value));

        if (_blacklistTrackTypes is { } blockedTypes)
            tracks = tracks.Where(t => t.Type != null && !blockedTypes.Contains(t.Type.Value));

        return tracks;
    }

    public bool IsTrackSuitable(Track track) =>
        IsSceneAllowed(track.Scene) && IsTypeAllowed(track.Type) && IsNameAllowed(track.Name);

    private bool IsSceneAllowed(Scene scene) =>
        _whitelistSceneIds is null || _whitelistSceneIds.Contains(scene.SceneId);

    private bool IsTypeAllowed(int? type)
    {
        if (_whitelistTrackTypes is null && _blacklistTrackTypes is null)
            return true;

        return type is { } knownType
               && (_whitelistTrackTypes is null || _whitelistTrackTypes.Contains(knownType))
               && (_blacklistTrackTypes is null || !_blacklistTrackTypes.Contains(knownType));
    }

    private bool IsNameAllowed(string name) =>
        _blacklistRegexes is null || !_blacklistRegexes.Any(regex => regex.IsMatch(name));
}
