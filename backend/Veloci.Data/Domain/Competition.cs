namespace Veloci.Data.Domain;

public class Competition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public DateTime StartedOn { get; set; } = DateTime.Now;

    public CompetitionState State { get; set; }

    public long ChatId { get; set; }

    public virtual Track Track { get; set; }
    public string TrackId { get; set; }

    public string? InitialResultsId { get; set; }
    public virtual TrackResults? InitialResults { get; set; }

    public string? CurrentResultsId { get; set; }
    public virtual TrackResults? CurrentResults { get; set; }

    public virtual List<TrackTimeDelta> TimeDeltas { get; set; }

    public virtual List<CompetitionResults> CompetitionResults { get; set; }

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

public static class IQueryableCompetionExtensions
{
    /// <summary>
    /// Filters competitions by date range
    /// </summary>
    /// <param name="query">Source query</param>
    /// <param name="from">From date inclusive</param>
    /// <param name="to">To date exclusive</param>
    /// <returns></returns>
    public static IQueryable<Competition> InRange(this IQueryable<Competition> query, DateTime from, DateTime to)
    {
        return query.Where(comp => comp.StartedOn >= from && comp.StartedOn < to);
    }


    /// <summary>
    /// Filters competitions that are not cancelled
    /// </summary>
    /// <param name="query">Source query</param>
    /// <returns></returns>
    public static IQueryable<Competition> NotCancelled(this IQueryable<Competition> query)
    {
        return query.Where(comp => comp.State != CompetitionState.Cancelled);
    }
}
