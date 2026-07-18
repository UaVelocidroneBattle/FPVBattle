using Microsoft.AspNetCore.Identity;

namespace Veloci.Data.Domain;

/// <summary>
/// An application account (admin or regular user signing in from the web/mobile apps).
/// Can be linked one-to-one to a <see cref="Pilot"/> once the user verifies ownership.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Full name from the identity provider (e.g. Google). Captured at sign-up.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// BCP 47 locale from the identity provider, e.g. "en" or "uk-UA".
    /// </summary>
    public string? Locale { get; set; }

    /// <summary>
    /// The Velocidrone pilot this account is linked to. A unique index guarantees
    /// a pilot cannot be claimed by two accounts.
    /// </summary>
    public int? PilotId { get; set; }

    public virtual Pilot? Pilot { get; set; }
}
