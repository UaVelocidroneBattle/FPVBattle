using FpvBattle.Application.Features.Patreon.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FpvBattle.Infrastructure.Patreon;

public static class PatreonServiceExtensions
{
    public static IServiceCollection AddPatreonApi(this IServiceCollection services)
    {
        services.AddScoped<IPatreonTokenManager, PatreonTokenManager>();
        services.AddScoped<IPatreonApiClient, PatreonApiClient>();
        services.AddScoped<IPatreonOAuthService, PatreonOAuthService>();

        // Configure HttpClient for Patreon API (used by PatreonApiClient)
        services.AddHttpClient<IPatreonApiClient, PatreonApiClient>("PatreonClient", client =>
        {
            client.BaseAddress = new Uri("https://www.patreon.com/api/oauth2/v2/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "FpvBattle/1.0");
        });

        // Configure HttpClient for PatreonController OAuth flow (used by PatreonTokenManager)
        services.AddHttpClient("PatreonOAuth", client =>
        {
            client.BaseAddress = new Uri("https://www.patreon.com/api/oauth2/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "FpvBattle/1.0");
        });

        return services;
    }
}
