using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Veloci.Logic.Features.Patreon.Models;

namespace Veloci.Logic.Features.Patreon.Services;

public class PatreonOAuthService : IPatreonOAuthService
{
    private readonly PatreonOptions _options;
    private readonly ILogger<PatreonOAuthService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public PatreonOAuthService(
        IOptions<PatreonOptions> options, 
        ILogger<PatreonOAuthService> logger, 
        IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public string GenerateAuthorizationUrl(string state)
    {
        var clientId = _options.ClientId;
        var redirectUri = _options.RedirectUri;

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
        {
            _logger.LogError("Patreon OAuth configuration is missing");
            throw new InvalidOperationException("Patreon configuration is not set up");
        }

        var scopes = "identity campaigns campaigns.members campaigns.members[email]";

        var authUrl = $"https://www.patreon.com/oauth2/authorize" +
                     $"?response_type=code" +
                     $"&client_id={clientId}" +
                     $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                     $"&scope={Uri.EscapeDataString(scopes)}" +
                     $"&state={state}";

        return authUrl;
    }

    public async Task<PatreonTokenResponse?> ExchangeCodeForTokensAsync(string code)
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