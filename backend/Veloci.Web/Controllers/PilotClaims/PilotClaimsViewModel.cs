namespace Veloci.Web.Controllers.PilotClaims;

public class PilotClaimsViewModel
{
    public List<PilotClaimRow> Claims { get; set; } = [];
}

public class PilotClaimRow
{
    public int ClaimId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string PilotName { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public DateTime ExpiresOn { get; set; }
    public bool IsExpired { get; set; }
    public bool PilotExists { get; set; }
    public bool PilotLinked { get; set; }
    public bool UserLinked { get; set; }

    /// <summary>An expired claim can still be approved — that is the admin override.</summary>
    public bool CanApprove => PilotExists && !PilotLinked && !UserLinked;
}
