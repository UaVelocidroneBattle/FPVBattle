using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Veloci.Logic.API.Options;

namespace Veloci.Web.Controllers;

public class PatreonController : Controller
{
    private readonly PatreonOptions _options;
    private readonly ILogger<PatreonController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public PatreonController(IOptions<PatreonOptions> options, ILogger<PatreonController> logger, IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
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

public class PatreonTokenResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public string TokenType { get; set; } = null!;
    public int ExpiresIn { get; set; }
    public string? Scope { get; set; }
}