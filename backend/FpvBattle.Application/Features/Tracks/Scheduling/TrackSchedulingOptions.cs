namespace FpvBattle.Application.Features.Tracks.Scheduling;

public class TrackSchedulingOptions
{
    public const string SectionName = "TrackScheduling";

    public bool Enabled { get; set; }

    /// <summary>
    /// Tracks a paying patron may schedule per calendar month, keyed by Patreon tier name. A tier not listed gets none
    /// </summary>
    public Dictionary<string, int> MonthlyLimitByTier { get; set; } = new();

    public int MonthlyLimitFor(string? tierName) =>
        tierName is not null && MonthlyLimitByTier.TryGetValue(tierName, out var limit) ? limit : 0;
}
