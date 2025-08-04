using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Patreon.Services;

/// <summary>
/// Manages Patreon OAuth tokens including storage, retrieval, and automatic refresh.
/// </summary>
public interface IPatreonTokenManager
{
    /// <summary>
    /// Retrieves current stored tokens from database.
    /// </summary>
    Task<PatreonTokens?> GetCurrentTokensAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Gets a valid access token, automatically refreshing if expired.
    /// </summary>
    Task<string?> GetValidAccessTokenAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Refreshes an expired access token using the refresh token.
    /// </summary>
    Task<string?> RefreshAccessTokenAsync(string refreshToken, CancellationToken ct = default);
    
    /// <summary>
    /// Updates stored tokens in database with new values from OAuth response.
    /// </summary>
    Task UpdateStoredTokensAsync(string accessToken, string refreshToken, int expiresIn, string? scope = null, CancellationToken ct = default);
}