namespace Veloci.Logic.Features.Auth.Models;

public class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>
    /// All Google OAuth client ids whose ID tokens are accepted (web, and later Android/iOS).
    /// </summary>
    public List<string> GoogleClientIds { get; set; } = [];

    public string JwtIssuer { get; set; } = "FpvBattle";

    public string JwtAudience { get; set; } = "FpvBattle";

    /// <summary>
    /// Symmetric HMAC key (32+ random bytes). Comes from user secrets / environment, never from the repo.
    /// </summary>
    public string JwtSigningKey { get; set; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    public int RefreshTokenLifetimeDays { get; set; } = 60;
}
