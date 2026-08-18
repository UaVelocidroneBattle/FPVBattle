using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Veloci.Logic.Features.Auth.Models;
using Veloci.Logic.Features.Auth.Services;

namespace Veloci.Logic.Features.Auth;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));

        services.AddScoped<GoogleAuthService>();
        services.AddScoped<TokenService>();

        var authOptions = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
        var signingKey = ResolveSigningKey(authOptions);
        services.PostConfigure<AuthOptions>(options => options.JwtSigningKey = signingKey);

        // No default scheme is passed to AddAuthentication, so Identity cookies
        // stay the default and the admin UI keeps working unchanged.
        services.AddAuthentication().AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = authOptions.JwtIssuer,
                ValidAudience = authOptions.JwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

        return services;
    }

    /// <summary>
    /// Falls back to an ephemeral random key when none is configured, so the app
    /// still starts in dev; issued tokens then become invalid on restart.
    /// </summary>
    private static string ResolveSigningKey(AuthOptions options)
    {
        if (string.IsNullOrEmpty(options.JwtSigningKey))
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        // HS256 requires a key of at least 256 bits; fail at startup rather than on first sign-in
        if (Encoding.UTF8.GetByteCount(options.JwtSigningKey) < 32)
            throw new InvalidOperationException(
                "Auth:JwtSigningKey must be at least 32 characters (256 bits) for HS256.");

        return options.JwtSigningKey;
    }
}
