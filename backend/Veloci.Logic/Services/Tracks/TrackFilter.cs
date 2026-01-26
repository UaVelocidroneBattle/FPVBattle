using System.Text.RegularExpressions;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Services.Tracks.Models;

namespace Veloci.Logic.Services.Tracks;

/// <summary>
/// Filters tracks based on configuration-driven patterns
/// </summary>
/// <remarks>
/// Supports both whitelist and blacklist patterns. When whitelist patterns are specified,
/// only tracks matching the whitelist are included. Otherwise, blacklist patterns exclude tracks.
/// This allows for cup-specific track filtering (e.g., 5-inch excludes whoops, whoop includes only micro drones).
/// </remarks>
public class TrackFilter
{
    private readonly Regex[]? _whitelistRegexes;
    private readonly Regex[]? _blacklistRegexes;

    public TrackFilter(TrackFilterOptions options)
    {
        // Compile whitelist patterns if specified
        if (options.WhitelistPatterns?.Any() == true)
        {
            _whitelistRegexes = options.WhitelistPatterns
                .Select(pattern => new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
                .ToArray();
        }

        // Compile blacklist patterns if specified
        if (options.BlacklistPatterns.Any())
        {
            _blacklistRegexes = options.BlacklistPatterns
                .Select(pattern => new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
                .ToArray();
        }
    }

    /// <summary>
    /// Determines if a track is suitable based on the filter configuration
    /// </summary>
    /// <param name="track">Track to evaluate</param>
    /// <returns>True if track passes the filter, false otherwise</returns>
    public bool IsTrackSuitable(ParsedTrackModel track)
    {
        // Whitelist takes precedence (for whoop cup)
        if (_whitelistRegexes != null)
        {
            return _whitelistRegexes.Any(regex => regex.IsMatch(track.Name));
        }

        // Otherwise use blacklist (for 5-inch cup)
        if (_blacklistRegexes != null)
        {
            return !_blacklistRegexes.Any(regex => regex.IsMatch(track.Name));
        }

        // No filters configured - allow all tracks
        return true;
    }
}
