namespace Veloci.Data.Domain;

public class PatreonTokens
{
    public int Id { get; set; }

    public string AccessToken { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;

    public int ExpiresIn { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime LastUpdated { get; set; }

    public string? Scope { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public bool IsExpiringSoon(int bufferMinutes = 10) => DateTime.UtcNow >= ExpiresAt.AddMinutes(-bufferMinutes);

    public void UpdateFromTokenResponse(string accessToken, string refreshToken, int expiresIn, string? scope = null)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresIn = expiresIn;
        Scope = scope;
        var now = DateTime.UtcNow;
        CreatedAt = now;
        LastUpdated = now;
        ExpiresAt = now.AddSeconds(expiresIn);
    }
}