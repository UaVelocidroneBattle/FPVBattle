using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Patreon.Models;

namespace Veloci.Logic.Features.Patreon.Services;

public class PatreonTokenManager : IPatreonTokenManager
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PatreonTokenManager> _logger;
    private readonly PatreonOptions _options;
    private readonly IRepository<PatreonTokens> _tokensRepository;

    public PatreonTokenManager(IHttpClientFactory httpClientFactory, IOptions<PatreonOptions> options,
        ILogger<PatreonTokenManager> logger, IRepository<PatreonTokens> tokensRepository)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
        _tokensRepository = tokensRepository;
    }

    public async Task<PatreonTokens?> GetCurrentTokensAsync()
    {
        try
        {
            // First try to get tokens from database (most up-to-date)
            var dbTokens = await _tokensRepository.GetAll().FirstOrDefaultAsync();

            if (dbTokens != null)
            {
                _logger.LogDebug("Using tokens from database");
                return dbTokens;
            }

            // Fallback to configuration tokens (initial setup)
            if (!string.IsNullOrEmpty(_options.AccessToken) && !string.IsNullOrEmpty(_options.RefreshToken))
            {
                _logger.LogDebug("Using tokens from configuration");
                var configTokens = new PatreonTokens();
                configTokens.UpdateFromTokenResponse(_options.AccessToken, _options.RefreshToken,
                    86400); // Assume 24h expiry
                return configTokens;
            }

            _logger.LogWarning("No Patreon tokens found in database or configuration");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current Patreon tokens");
            return null;
        }
    }

    public async Task<string?> GetValidAccessTokenAsync()
    {
        try
        {
            var tokens = await GetCurrentTokensAsync();
            if (tokens == null)
            {
                _logger.LogWarning("No Patreon tokens available");
                return null;
            }

            // Check if token is expired or expiring soon (10 minute buffer)
            if (tokens.IsExpiringSoon())
            {
                _logger.LogInformation("Patreon access token is expiring soon, refreshing...");

                var newAccessToken = await RefreshAccessTokenAsync(tokens.RefreshToken);
                if (newAccessToken != null)
                {
                    return newAccessToken;
                }

                _logger.LogWarning("Failed to refresh Patreon access token, using existing token");
            }

            return tokens.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting valid Patreon access token");
            return null;
        }
    }

    public async Task<string?> RefreshAccessTokenAsync(string refreshToken)
    {
        try
        {
            if (string.IsNullOrEmpty(_options.ClientId) || string.IsNullOrEmpty(_options.ClientSecret))
            {
                _logger.LogError("Patreon client credentials not configured");
                return null;
            }

            var requestBody = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret
            };

            using var oauthClient = _httpClientFactory.CreateClient("PatreonOAuth");
            var response = await oauthClient.PostAsync("token", new FormUrlEncodedContent(requestBody));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to refresh Patreon access token: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<PatreonTokenResponse>(json,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

            if (tokenData != null)
            {
                // Store the refreshed tokens in database
                await UpdateStoredTokensAsync(tokenData.AccessToken, tokenData.RefreshToken, tokenData.ExpiresIn,
                    tokenData.Scope);
                _logger.LogInformation("Successfully refreshed and stored Patreon access token");
                return tokenData.AccessToken;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing Patreon access token");
            return null;
        }
    }

    public async Task UpdateStoredTokensAsync(string accessToken, string refreshToken, int expiresIn,
        string? scope = null)
    {
        try
        {
            var existingTokens = await _tokensRepository.GetAll().FirstOrDefaultAsync();

            if (existingTokens != null)
            {
                existingTokens.UpdateFromTokenResponse(accessToken, refreshToken, expiresIn, scope);
                await _tokensRepository.UpdateAsync(existingTokens);
            }
            else
            {
                var newTokens = new PatreonTokens();
                newTokens.UpdateFromTokenResponse(accessToken, refreshToken, expiresIn, scope);
                await _tokensRepository.AddAsync(newTokens);
            }

            await _tokensRepository.SaveChangesAsync();
            _logger.LogInformation("Updated Patreon tokens in database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing Patreon tokens to database");
        }
    }
}