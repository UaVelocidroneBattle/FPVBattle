using System.Diagnostics;

namespace Veloci.Data.Domain;

[DebuggerDisplay("{Name} - {TierName} ({Status})")]
public class PatreonSupporter
{
    public string PatreonId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string? TierName { get; set; }

    public decimal? Amount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime FirstSupportedAt { get; set; }

    public DateTime LastUpdated { get; set; }
}