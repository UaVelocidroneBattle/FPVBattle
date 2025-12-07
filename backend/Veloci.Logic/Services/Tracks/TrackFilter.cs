using System.Text.RegularExpressions;
using Veloci.Logic.Services.Tracks.Models;

namespace Veloci.Logic.Services.Tracks;

public class TrackFilter
{
    private static readonly Regex[] BlackListedTracks =
    {
        new ("Pylons", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new ("Freestyle", RegexOptions.Compiled | RegexOptions.IgnoreCase),
    };


    public bool IsTrackGood(ParsedTrackModel track)
    {
        return !BlackListedTracks.Any(b => b.IsMatch(track.Name));
    }
}
