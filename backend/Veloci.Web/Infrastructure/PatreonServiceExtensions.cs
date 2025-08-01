using Veloci.Logic.Services;

namespace Veloci.Web.Infrastructure;

public static class PatreonServiceExtensions
{
    public static IServiceCollection AddPatreonServices(this IServiceCollection services)
    {
        services.AddScoped<IPatreonService, PatreonService>();
        services.AddScoped<PatreonSyncJob>();

        // Configure HttpClient for Patreon API
        services.AddHttpClient<IPatreonService, PatreonService>("PatreonClient", client =>
        {
            client.BaseAddress = new Uri("https://www.patreon.com/api/oauth2/v2/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "VelocidroneBot/1.0");
        });

        // Configure HttpClient for PatreonController OAuth flow
        services.AddHttpClient("PatreonOAuth", client =>
        {
            client.BaseAddress = new Uri("https://www.patreon.com/api/oauth2/");
            client.Timeout = TimeSpan.FromSeconds(30);    
            client.DefaultRequestHeaders.Add("User-Agent", "VelocidroneBot/1.0");
        });

        return services;
    }
}