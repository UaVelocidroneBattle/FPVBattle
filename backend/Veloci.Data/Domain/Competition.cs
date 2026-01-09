namespace Veloci.Data.Domain;

/// <summary>
/// Represents a daily racing competition on a specific track.
/// </summary>
/// <remarks>
/// <para>
/// A competition is created each day when a new track is selected and announced
/// in the chat channels. Throughout the day, the bot monitors pilot times on
/// the Velocidrone leaderboard, tracking progress via <see cref="InitialResults"/>
/// (snapshot at start) and <see cref="CurrentResults"/> (latest times).
/// </para>
/// <para>
/// At competition close, final rankings are calculated and stored in
/// <see cref="CompetitionResults"/>, and pilots earn points based on their placement.
/// </para>
/// </remarks>
public class Competition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public DateTime StartedOn { get; set; } = DateTime.Now;

    public CompetitionState State { get; set; }

    public long ChatId { get; set; }

    public virtual Track Track { get; set; }
    public string TrackId { get; set; }

    public virtual TrackResults InitialResults { get; set; }

    public virtual TrackResults CurrentResults { get; set; }

    /// <summary>
    /// Append-only log of all lap time changes recorded during this competition.
    /// </summary>
    /// <remarks>
    /// Each time the bot polls Velocidrone and detects a pilot's time has changed,
    /// a new <see cref="TrackTimeDelta"/> is appended. This provides:
    /// <list type="bullet">
    ///   <item><description>Complete history of improvements throughout the day</description></item>
    ///   <item><description>Data source for real-time chat notifications</description></item>
    ///   <item><description>Input for leaderboard calculation (best time per pilot)</description></item>
    /// </list>
    /// <para>
    /// Deltas are computed by <c>RaceResultDeltaAnalyzer.CompareResults()</c> which
    /// compares <see cref="CurrentResults"/> against fresh API data.
    /// </para>
    /// </remarks>
    public virtual List<TrackTimeDelta> TimeDeltas { get; set; }

    public virtual List<CompetitionResults> CompetitionResults { get; set; }

    /// <summary>
    /// Flexible key-value store for competition-scoped metadata.
    /// </summary>
    /// <remarks>
    /// Used to store platform-specific or feature-specific data that doesn't warrant
    /// a dedicated column. For example, Discord stores the leaderboard message ID here
    /// so it can edit the message with updates rather than posting new ones.
    /// <para>
    /// Use <see cref="AddOrUpdateVariable"/> and <see cref="GetVariable"/> to access.
    /// Variable names should be defined as constants in <see cref="CompetitionVariables"/>.
    /// </para>
    /// </remarks>
    public virtual List<CompetitionVariable> Variables { get; set; }

    public bool ResultsPosted { get; set; }

    public void AddOrUpdateVariable(string name, object value)
    {
        Variables ??= new List<CompetitionVariable>();

        var variable = Variables.FirstOrDefault(v => v.Name == name);

        if (variable is null)
        {
            variable = new CompetitionVariable
            {
                Name = name,
                CompetitionId = Id
            };

            variable.UpdateValue(value);
            Variables.Add(variable);
        }
        else
        {
            variable.UpdateValue(value);
        }
    }

    public CompetitionVariable? GetVariable(string name)
    {
        return Variables.FirstOrDefault(v => v.Name == name);
    }

    public bool IsPilotAtLocalRank(Pilot pilot, int rank)
    {
        return CompetitionResults.GetByLocalRank(rank)?.Pilot.Id == pilot.Id;
    }

    public CompetitionResults? GetSlowest()
    {
        return CompetitionResults
            .OrderBy(res => res.LocalRank)
            .LastOrDefault();
    }
}

public static class IQueryableCompetitionExtensions
{
    /// <param name="query">Source query</param>
    extension(IQueryable<Competition> query)
    {
        /// <summary>
        /// Filters competitions by date range
        /// </summary>
        /// <param name="from">From date inclusive</param>
        /// <param name="to">To date exclusive</param>
        /// <returns></returns>
        public IQueryable<Competition> InRange(DateTime from, DateTime to)
        {
            return query.Where(comp => comp.StartedOn >= from && comp.StartedOn < to);
        }

        /// <summary>
        /// Filters competitions that are not cancelled
        /// </summary>
        /// <returns></returns>
        public IQueryable<Competition> NotCancelled()
        {
            return query.Where(comp => comp.State != CompetitionState.Cancelled);
        }
    }
}
