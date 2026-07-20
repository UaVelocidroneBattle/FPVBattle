namespace Veloci.Data.Domain;

/// <summary>
/// A long-lived opaque token allowing a client to obtain new access tokens.
/// Only the SHA-256 hash of the token is stored.
/// </summary>
public class RefreshToken
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; } = null!;

    public string TokenHash { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime ExpiresOn { get; set; }

    public DateTime? RevokedOn { get; set; }

    public bool IsActive => RevokedOn is null && DateTime.UtcNow < ExpiresOn;
}
