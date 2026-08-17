namespace Veloci.Web.Controllers.Api.Statistics;

/// <summary>
/// Size of the pilot community over time.
/// </summary>
/// <param name="Days">One <c>yyyy-MM-dd</c> label per point, ascending and without gaps.</param>
/// <param name="Totals">Total pilots at the end of each day, aligned to <paramref name="Days"/>.</param>
public record PilotsCountModel(IReadOnlyList<string> Days, IReadOnlyList<int> Totals);

/// <summary>
/// Pilots who joined in each month.
/// </summary>
/// <param name="Months">One <c>yyyy-MM</c> label per bar, ascending and without gaps.</param>
/// <param name="Counts">New pilots in each month, aligned to <paramref name="Months"/>.</param>
public record NewPilotsCountModel(IReadOnlyList<string> Months, IReadOnlyList<int> Counts);

/// <summary>
/// Pilots who flew each day, one series per cup.
/// </summary>
/// <param name="Days">One <c>yyyy-MM-dd</c> label per point, ascending and without gaps.</param>
/// <param name="Series">One entry per configured cup, in configuration order.</param>
public record ParticipationModel(IReadOnlyList<string> Days, IReadOnlyList<CupParticipationModel> Series);

/// <summary>
/// A single cup's numbers, aligned to <see cref="ParticipationModel.Days"/>.
/// </summary>
/// <param name="Counts">Pilots who flew, or <c>null</c> on days the cup held no competition.</param>
public record CupParticipationModel(string CupId, string CupName, IReadOnlyList<int?> Counts);
