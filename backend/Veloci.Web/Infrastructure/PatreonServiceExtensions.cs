using Veloci.Logic.API.Options;
using Veloci.Logic.Services;

namespace Veloci.Web.Infrastructure;

public static class PatreonServiceExtensions
{
    public static IServiceCollection AddPatreonServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Patreon options
        services.Configure<PatreonOptions>(configuration.GetSection(PatreonOptions.SectionName));

        // Register core services
        services.AddScoped<IPatreonTokenManager, PatreonTokenManager>();
        services.AddScoped<IPatreonApiClient, PatreonApiClient>();
        services.AddScoped<IPatreonService, PatreonService>();
        services.AddScoped<PatreonSyncJob>();

        // Configure HttpClient for Patreon API (used by PatreonApiClient)
        services.AddHttpClient<IPatreonApiClient, PatreonApiClient>("PatreonClient", client =>
        {
            client.BaseAddress = new Uri("https://www.patreon.com/api/oauth2/v2/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "VelocidroneBot/1.0");
        });

        // Configure HttpClient for PatreonController OAuth flow (used by PatreonTokenManager)
        services.AddHttpClient("PatreonOAuth", client =>
        {
            client.BaseAddress = new Uri("https://www.patreon.com/api/oauth2/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "VelocidroneBot/1.0");
        });

        return services;
    }
}
