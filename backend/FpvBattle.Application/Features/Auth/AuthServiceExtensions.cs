using FpvBattle.Application.Features.Auth.Models;
using FpvBattle.Application.Features.Auth.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FpvBattle.Application.Features.Auth;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));

        services.AddScoped<GoogleAuthService>();
        services.AddScoped<TokenService>();

        return services;
    }
}
