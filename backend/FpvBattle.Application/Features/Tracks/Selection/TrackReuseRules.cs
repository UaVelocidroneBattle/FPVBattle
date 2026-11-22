using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Tracks.Selection;

/// <summary>
/// When a track may be raced again: it must not be rated badly by pilots, nor raced within the cooldown.
/// The random pick, the admin track queue and patron scheduling all apply these rules.
/// </summary>
public static class TrackReuseRules
{
    public const double MinimumGoodRating = 3;
    public const int CooldownMonths = 6;

    public static DateTime CooldownStart => DateTime.Now.AddMonths(-CooldownMonths);

    public static bool IsWellRated(TrackRating? rating) => rating?.Value is null or >= MinimumGoodRating;

    public static bool IsInCooldown(DateTime racedOn) => racedOn > CooldownStart;

    public static DateTime AvailableAgainOn(DateTime racedOn) => racedOn.AddMonths(CooldownMonths);
}
