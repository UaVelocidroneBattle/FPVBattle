using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Application.Features.Tracks.Scheduling;

/// <summary>
/// Finds tracks by part of their name for the search boxes that queue a track, in the database:
/// names starting with the term come first, then by name and scene, at most <see cref="MaxResults"/>.
/// </summary>
public static class TrackSearch
{
    public const int MinTermLength = 2;
    public const int MaxResults = 40;

    private const string LikeEscape = "\\";

    public static bool IsSearchable(string term) => term.Trim().Length >= MinTermLength;

    // SQLite's LIKE ignores case for Latin letters only
    public static IQueryable<Track> SearchByName(this IQueryable<Track> tracks, string term)
    {
        var text = EscapeLikeWildcards(term.Trim());
        var containing = $"%{text}%";
        var startingWith = $"{text}%";

        return tracks
            .Where(t => EF.Functions.Like(t.Name, containing, LikeEscape))
            .OrderByDescending(t => EF.Functions.Like(t.Name, startingWith, LikeEscape))
            .ThenBy(t => t.Name)
            .ThenBy(t => t.Scene.Name)
            .Take(MaxResults);
    }

    private static string EscapeLikeWildcards(string text) => text
        .Replace(LikeEscape, LikeEscape + LikeEscape)
        .Replace("%", LikeEscape + "%")
        .Replace("_", LikeEscape + "_");
}
