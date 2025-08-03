using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Patreon.Services;

public interface IPatreonTokenManager
{
    Task<PatreonTokens?> GetCurrentTokensAsync(CancellationToken ct = default);
    Task<string?> GetValidAccessTokenAsync(CancellationToken ct = default);
    Task<string?> RefreshAccessTokenAsync(string refreshToken, CancellationToken ct = default);
    Task UpdateStoredTokensAsync(string accessToken, string refreshToken, int expiresIn, string? scope = null, CancellationToken ct = default);
}