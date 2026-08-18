namespace Veloci.Logic.Services.Statistics;

/// <summary>
/// Daily pilot participation over a continuous span of days, with one series per cup.
/// </summary>
/// <param name="Days">
/// Every calendar day from the first to the last competition, without gaps, ascending.
/// Acts as the shared spine all series are aligned to.
/// </param>
/// <param name="Series">One entry per cup configured in appsettings, in configuration order.</param>
public record DailyParticipation(IReadOnlyList<DateTime> Days, IReadOnlyList<CupParticipationSeries> Series)
{
    public static DailyParticipation Empty { get; } = new([], []);
}

/// <summary>
/// Pilot counts of a single cup, aligned to <see cref="DailyParticipation.Days"/>.
/// </summary>
/// <param name="CupId">Cup identifier, e.g. "open-class".</param>
/// <param name="CupName">Display name from the cup configuration, e.g. "Open Class".</param>
/// <param name="Counts">
/// Number of pilots who posted a result on each day, or <c>null</c> for days
/// the cup held no competition. Same length and order as the shared spine.
/// </param>
public record CupParticipationSeries(string CupId, string CupName, IReadOnlyList<int?> Counts);
