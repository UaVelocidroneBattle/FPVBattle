namespace Veloci.Data.Domain;

/// <summary>
/// A user's pending request to link their account to a Velocidrone pilot.
/// The claim is verified ("fly-to-verify") when a pilot with the claimed name
/// posts a result on a daily track while the claim is active.
/// </summary>
public class PilotClaim
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string PilotName { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime ExpiresOn { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
}
