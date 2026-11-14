using System.Net.Http.Headers;
using FpvBattle.Application.API.Velocidrone.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FpvBattle.Application.API.Velocidrone;

public static class VelocidroneServiceExtensions
{
    public static IServiceCollection AddVelocidroneClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ApiSettings>()
            .Bind(configuration.GetSection(ApiSettings.SectionName))
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.Token), "Velocidrone API token (API:Token) is not configured");

        services.AddHttpClient<VelocidroneClient>((provider, client) =>
        {
            var token = provider.GetRequiredService<IOptions<ApiSettings>>().Value.Token;

            client.BaseAddress = VelocidroneClient.BaseAddress;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        });

        return services;
    }
}
