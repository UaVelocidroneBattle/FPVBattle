namespace Veloci.Logic.Features.Patreon.Models;

/// <summary>
///     Represents the OAuth2 token response from Patreon's token exchange endpoint.
///     Contains authentication tokens required for accessing the Patreon API.
/// </summary>
public class PatreonTokenResponse
{
    /// <summary>
    ///     Single-use access token for authenticating API requests to Patreon.
    ///     This token expires after the duration specified in ExpiresIn.
    /// </summary>
    public string AccessToken { get; set; } = null!;

    /// <summary>
    ///     Single-use refresh token for obtaining a new access token when the current one expires.
    ///     Should be stored securely and used only when the access token needs renewal.
    /// </summary>
    public string RefreshToken { get; set; } = null!;

    /// <summary>
    ///     The type of token issued, always "Bearer" for Patreon OAuth2 implementation.
    /// </summary>
    public string TokenType { get; set; } = null!;

    /// <summary>
    ///     Token lifetime duration in seconds indicating how long the access token remains valid.
    ///     After this time, the refresh token must be used to obtain a new access token.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    ///     Token permissions defining what resources and actions the token can access.
    ///     Typically includes "identity campaigns.members" for accessing supporter data.
    /// </summary>
    public string? Scope { get; set; }
}
