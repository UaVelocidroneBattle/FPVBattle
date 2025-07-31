using System.ComponentModel.DataAnnotations;

namespace Veloci.Data.Domain;

public class PatreonSupporter
{
    [Key]
    [MaxLength(128)]
    public string PatreonId { get; set; } = null!;

    [MaxLength(256)]
    public string Name { get; set; } = null!;

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(128)]
    public string? TierName { get; set; }

    public decimal? Amount { get; set; }

    [MaxLength(64)]
    public string Status { get; set; } = null!;

    public DateTime FirstSupportedAt { get; set; }

    public DateTime LastUpdated { get; set; }
}