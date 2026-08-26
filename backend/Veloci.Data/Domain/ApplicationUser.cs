using Microsoft.AspNetCore.Identity;

namespace Veloci.Data.Domain;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    /// <summary>
    /// BCP 47 locale from the identity provider, e.g. "en" or "uk-UA".
    /// </summary>
    public string? Locale { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public int? PilotId { get; set; }
    public virtual Pilot? Pilot { get; set; }
}
