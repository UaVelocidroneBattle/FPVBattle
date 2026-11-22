namespace FpvBattle.Application.Features.Tracks.Scheduling;

public enum PatronQuotaBlocker
{
    PilotNotLinked,
    NotPatron,
    TierNotIncluded,
    MonthlyLimitReached
}

/// <summary>How many tracks a user may schedule this month, and why they can't schedule now, if so.</summary>
public sealed record PatronQuota(PatronQuotaBlocker? Blocker, int MonthlyLimit, int UsedThisMonth, DateTime NextMonthStart)
{
    public int Remaining => Math.Max(0, MonthlyLimit - UsedThisMonth);
}
