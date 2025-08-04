using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Patreon.Services;

/// <summary>
///     Manages Patreon OAuth tokens including storage, retrieval, and automatic refresh.
/// </summary>
public interface IPatreonTokenManager
{
    /// <summary>
    ///     Retrieves current stored tokens from database or configuration fallback.
    /// </summary>
    /// <exception cref="PatreonTokenUnavailableException">Thrown when no tokens are configured anywhere</exception>
    Task<PatreonTokens> GetCurrentTokensAsync(CancellationToken ct = default);

    /// <summary>
    ///     Gets a valid access token, automatically refreshing if expired.
    /// </summary>
    /// <exception cref="PatreonTokenUnavailableException">Thrown when no tokens are configured</exception>
    /// <exception cref="PatreonTokenRefreshException">Thrown when token refresh fails</exception>
    Task<string> GetValidAccessTokenAsync(CancellationToken ct = default);

    /// <summary>
    ///     Refreshes an expired access token using the refresh token.
    /// </summary>
    /// <exception cref="PatreonTokenRefreshException">Thrown when token refresh fails</exception>
    Task<string> RefreshAccessTokenAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>
    ///     Updates stored tokens in database with new values from OAuth response.
    /// </summary>
    /// <exception cref="PatreonTokenStorageException">Thrown when database storage operation fails</exception>
    Task UpdateStoredTokensAsync(string accessToken, string refreshToken, int expiresIn, string? scope = null,
        CancellationToken ct = default);
}
