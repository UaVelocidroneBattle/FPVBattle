using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Veloci.Logic.API.Options;
using Veloci.Logic.Services;

namespace Veloci.Web.Controllers;

public class PatreonController : Controller
{
    private readonly PatreonOptions _options;
    private readonly ILogger<PatreonController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPatreonTokenManager _patreonTokenManager;

    public PatreonController(IOptions<PatreonOptions> options, ILogger<PatreonController> logger, IHttpClientFactory httpClientFactory, IPatreonTokenManager patreonTokenManager)
    {
        _options = options.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _patreonTokenManager = patreonTokenManager;
    }

    public IActionResult Connect()
    {
        var clientId = _options.ClientId;
        var redirectUri = _options.RedirectUri;

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
        {
            _logger.LogError("Patreon OAuth configuration is missing");
            return BadRequest("Patreon configuration is not set up");
        }

        var scopes = "identity campaigns.members";
        var state = Guid.NewGuid().ToString();

        var authUrl = $"https://www.patreon.com/oauth2/authorize" +
                     $"?response_type=code" +
                     $"&client_id={clientId}" +
                     $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                     $"&scope={Uri.EscapeDataString(scopes)}" +
                     $"&state={state}";

        return Redirect(authUrl);
    }

    public async Task<IActionResult> Callback(string? code, string? state, string? error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogError("Patreon OAuth error: {Error}", error);
            return BadRequest($"Authorization failed: {error}");
        }

        if (string.IsNullOrEmpty(code))
        {
            _logger.LogError("Authorization code not received from Patreon");
            return BadRequest("Authorization code not received");
        }

        try
        {
            var tokenResponse = await ExchangeCodeForTokens(code);
            if (tokenResponse == null)
            {
                return BadRequest("Failed to exchange authorization code for tokens");
            }

            await _patreonTokenManager.UpdateStoredTokensAsync(tokenResponse.AccessToken, tokenResponse.RefreshToken, tokenResponse.ExpiresIn, tokenResponse.Scope);

            return View("Tokens", tokenResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Patreon OAuth callback");
            return BadRequest("An error occurred during authorization");
        }
    }

    private async Task<PatreonTokenResponse?> ExchangeCodeForTokens(string code)
    {
        try
        {
            var clientId = _options.ClientId;
            var clientSecret = _options.ClientSecret;
            var redirectUri = _options.RedirectUri;

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
            {
                _logger.LogError("Patreon OAuth configuration is incomplete");
                return null;
            }

            var requestBody = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri
            };

            using var client = _httpClientFactory.CreateClient("PatreonOAuth");
            var response = await client.PostAsync("token", new FormUrlEncodedContent(requestBody));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to exchange code for tokens. Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, errorContent);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<PatreonTokenResponse>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            return tokenData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging authorization code for tokens");
            return null;
        }
    }
}

/// <summary>
/// Represents the OAuth2 token response from Patreon's token exchange endpoint.
/// Contains authentication tokens required for accessing the Patreon API.
/// </summary>
public class PatreonTokenResponse
{
    /// <summary>
    /// Single-use access token for authenticating API requests to Patreon.
    /// This token expires after the duration specified in ExpiresIn.
    /// </summary>
    public string AccessToken { get; set; } = null!;
    
    /// <summary>
    /// Single-use refresh token for obtaining a new access token when the current one expires.
    /// Should be stored securely and used only when the access token needs renewal.
    /// </summary>
    public string RefreshToken { get; set; } = null!;
    
    /// <summary>
    /// The type of token issued, always "Bearer" for Patreon OAuth2 implementation.
    /// </summary>
    public string TokenType { get; set; } = null!;
    
    /// <summary>
    /// Token lifetime duration in seconds indicating how long the access token remains valid.
    /// After this time, the refresh token must be used to obtain a new access token.
    /// </summary>
    public int ExpiresIn { get; set; }
    
    /// <summary>
    /// Token permissions defining what resources and actions the token can access.
    /// Typically includes "identity campaigns.members" for accessing supporter data.
    /// </summary>
    public string? Scope { get; set; }
}
