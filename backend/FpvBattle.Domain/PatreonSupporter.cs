using System.Diagnostics;

namespace FpvBattle.Domain;

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

    public DateTime? FirstPaidAt { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual Pilot Pilot { get; set;  }

    public int? PilotId { get; set; }

    public bool IsPaying => Status == ActivePatronStatus && Amount > 0;

    public string? NormalizedEmail => Email?.ToUpperInvariant();

    public bool TryRecordFirstPayment(DateTime now)
    {
        if (!IsPaying || FirstPaidAt is not null)
        {
            return false;
        }

        FirstPaidAt = now;

        return true;
    }
}
