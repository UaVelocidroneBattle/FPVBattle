using Veloci.Logic.Features.Patreon.Models;

namespace Veloci.Logic.Features.Patreon.Services;

public interface IPatreonOAuthService
{
    /// <summary>
    ///     Generates the authorization URL for Patreon OAuth flow
    /// </summary>
    /// <param name="state">State parameter for CSRF protection</param>
    /// <returns>Authorization URL to redirect user to</returns>
    string GenerateAuthorizationUrl(string state);

    /// <summary>
    ///     Exchanges the authorization code for access and refresh tokens
    /// </summary>
    /// <param name="code">Authorization code from Patreon callback</param>
    /// <returns>Token response containing access token, refresh token, and metadata</returns>
    Task<PatreonTokenResponse?> ExchangeCodeForTokensAsync(string code);
}
