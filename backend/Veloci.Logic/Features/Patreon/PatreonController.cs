using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Veloci.Logic.Features.Patreon.Models;
using Veloci.Logic.Features.Patreon.Services;

namespace Veloci.Logic.Features.Patreon;

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

        var scopes = "identity campaigns campaigns.members campaigns.members[email]";
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

            //await _patreonTokenManager.UpdateStoredTokensAsync(tokenResponse.AccessToken, tokenResponse.RefreshToken, tokenResponse.ExpiresIn, tokenResponse.Scope);

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
