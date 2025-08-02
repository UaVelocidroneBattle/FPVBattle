using Veloci.Data.Domain;

namespace Veloci.Logic.Services;

public interface IPatreonTokenManager
{
    Task<PatreonTokens?> GetCurrentTokensAsync();
    Task<string?> GetValidAccessTokenAsync();
    Task<string?> RefreshAccessTokenAsync(string refreshToken);
    Task UpdateStoredTokensAsync(string accessToken, string refreshToken, int expiresIn, string? scope = null);
}
