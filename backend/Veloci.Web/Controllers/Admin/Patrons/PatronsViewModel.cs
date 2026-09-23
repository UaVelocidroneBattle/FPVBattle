namespace Veloci.Web.Controllers.Admin.Patrons;

public class PatronsViewModel
{
    public List<PatronRow> Patrons { get; set; } = [];
    public List<string> UnlinkedPilotNames { get; set; } = [];
}

public class PatronRow
{
    public string PatreonId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? TierName { get; set; }
    public decimal? Amount { get; set; }
    public bool IsActive { get; set; }
    public DateTime FirstSupportedAt { get; set; }
    public string? PilotName { get; set; }
    public string? SuggestedPilotName { get; set; }
    public bool HasMatchingUserWithoutPilot { get; set; }
}
