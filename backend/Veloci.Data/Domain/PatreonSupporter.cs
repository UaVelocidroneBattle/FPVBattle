using System.Diagnostics;

namespace Veloci.Data.Domain;

[DebuggerDisplay("{Name} - {TierName} ({Status})")]
public class PatreonSupporter
{
    public const string ActivePatronStatus = "active_patron";

    public string PatreonId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string? TierName { get; set; }

    public decimal? Amount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime FirstSupportedAt { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual Pilot Pilot { get; set;  }
    public int? PilotId { get; set; }

    public bool IsPaying => Status == ActivePatronStatus && Amount > 0;
}

public static class PatreonSupporterExtensions
{
    public static IQueryable<PatreonSupporter> Paying(this IQueryable<PatreonSupporter> query)
    {
        return query.Where(s => s.Status == PatreonSupporter.ActivePatronStatus && s.Amount > 0);
    }
}
