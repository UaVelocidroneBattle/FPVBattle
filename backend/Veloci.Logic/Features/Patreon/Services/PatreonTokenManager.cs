using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Patreon.Exceptions;
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

    public async Task<PatreonTokens> GetCurrentTokensAsync(CancellationToken ct = default)
    {
        try
        {
            // First try to get tokens from database (most up-to-date)
            var dbTokens = await _tokensRepository.GetAll().FirstOrDefaultAsync(ct);

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

            throw new PatreonTokenUnavailableException("No Patreon tokens configured in database or configuration");
        }
        catch (PatreonTokenUnavailableException)
        {
            throw; // Re-throw our specific exceptions
        }
        catch (OperationCanceledException)
        {
            throw; // Let cancellation bubble up
        }
        catch (Exception ex)
        {
            throw new PatreonTokenUnavailableException("Failed to retrieve Patreon tokens from database", ex);
        }
    }

    public async Task<string> GetValidAccessTokenAsync(CancellationToken ct = default)
    {
        var tokens = await GetCurrentTokensAsync(ct);

        // Check if token is expired or expiring soon (10 minute buffer)
        if (tokens.IsExpiringSoon())
        {
            _logger.LogInformation("Patreon access token expiring soon, refreshing...");

            try
            {
                return await RefreshAccessTokenAsync(tokens.RefreshToken, ct);
            }
            catch (PatreonTokenRefreshException)
            {
                _logger.LogWarning("Token refresh failed, using existing token");
                // Continue with existing token as fallback
            }
        }

        return tokens.AccessToken;
    }

    public async Task<string> RefreshAccessTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_options.ClientId) || string.IsNullOrEmpty(_options.ClientSecret))
        {
            throw new PatreonTokenRefreshException("Patreon client credentials not configured")
            {
                RefreshToken = refreshToken
            };
        }

        var requestBody = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        };

        try
        {
            using var oauthClient = _httpClientFactory.CreateClient("PatreonOAuth");
            var response = await oauthClient.PostAsync("token", new FormUrlEncodedContent(requestBody), ct);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(ct);
                throw new PatreonTokenRefreshException($"Failed to refresh Patreon access token: {response.StatusCode}")
                {
                    StatusCode = response.StatusCode,
                    Endpoint = "token",
                    ResponseContent = responseContent,
                    RefreshToken = refreshToken
                };
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var tokenData = JsonSerializer.Deserialize<PatreonTokenResponse>(json,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

            if (tokenData?.AccessToken == null || tokenData.RefreshToken == null)
            {
                throw new PatreonTokenRefreshException("Invalid token response from Patreon API")
                {
                    Endpoint = "token",
                    ResponseContent = json,
                    RefreshToken = refreshToken
                };
            }

            // Store the refreshed tokens in database
            await UpdateStoredTokensAsync(tokenData.AccessToken, tokenData.RefreshToken, tokenData.ExpiresIn,
                tokenData.Scope, ct);
            _logger.LogInformation("Successfully refreshed and stored Patreon access token");
            return tokenData.AccessToken;
        }
        catch (PatreonTokenRefreshException)
        {
            throw; // Re-throw our specific exceptions
        }
        catch (Exception ex)
        {
            throw new PatreonTokenRefreshException("Error refreshing Patreon access token", ex)
            {
                RefreshToken = refreshToken
            };
        }
    }

    public async Task UpdateStoredTokensAsync(string accessToken, string refreshToken, int expiresIn,
        string? scope = null, CancellationToken ct = default)
    {
        bool isUpdate = false;
        
        try
        {
            var existingTokens = await _tokensRepository.GetAll().FirstOrDefaultAsync(ct);
            isUpdate = existingTokens != null;

            if (existingTokens != null)
            {
                existingTokens.UpdateFromTokenResponse(accessToken, refreshToken, expiresIn, scope);
                await _tokensRepository.UpdateAsync(existingTokens);
                await _tokensRepository.SaveChangesAsync(ct);
                _logger.LogInformation("Updated existing Patreon tokens in database");
            }
            else
            {
                var newTokens = new PatreonTokens();
                newTokens.UpdateFromTokenResponse(accessToken, refreshToken, expiresIn, scope);
                await _tokensRepository.AddAsync(newTokens);
                await _tokensRepository.SaveChangesAsync(ct);
                _logger.LogInformation("Stored new Patreon tokens in database");
            }
        }
        catch (PatreonTokenStorageException)
        {
            throw; // Re-throw our specific exceptions
        }
        catch (OperationCanceledException)
        {
            throw; // Let cancellation bubble up
        }
        catch (Exception ex)
        {
            throw new PatreonTokenStorageException($"Failed to {(isUpdate ? "update" : "store")} Patreon tokens in database", ex)
            {
                IsUpdate = isUpdate
            };
        }
    }
}
