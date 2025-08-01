using Veloci.Data.Domain;

namespace Veloci.Logic.Services;

public interface IPatreonService
{
    Task<List<PatreonSupporter>> GetCampaignMembersAsync();
    Task<string?> RefreshAccessTokenAsync(string refreshToken);
    Task<PatreonTokens?> GetCurrentTokensAsync();
    Task<string?> GetValidAccessTokenAsync();
    Task UpdateStoredTokensAsync(string accessToken, string refreshToken, int expiresIn, string? scope = null);
}