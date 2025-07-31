using Veloci.Data.Domain;

namespace Veloci.Logic.Services;

public interface IPatreonService
{
    Task<List<PatreonSupporter>> GetCampaignMembersAsync();
    Task<string?> RefreshAccessTokenAsync(string refreshToken);
}