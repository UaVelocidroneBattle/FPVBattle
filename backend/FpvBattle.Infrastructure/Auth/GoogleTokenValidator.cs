using FpvBattle.Application.Features.Auth.Models;
using FpvBattle.Application.Features.Auth.Services;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Serilog;

namespace FpvBattle.Infrastructure.Auth;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private static readonly ILogger Log = Serilog.Log.ForContext<GoogleTokenValidator>();

    private readonly IOptions<AuthOptions> _options;

    public GoogleTokenValidator(IOptions<AuthOptions> options)
    {
        _options = options;
    }

    public async Task<GoogleIdentity?> ValidateAsync(string idToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = _options.Value.GoogleClientIds
            });

            return new GoogleIdentity(payload.Subject, payload.Email, payload.EmailVerified, payload.Name, payload.Locale);
        }
        catch (InvalidJwtException ex)
        {
            Log.Warning(ex, "Rejected invalid Google ID token");
            return null;
        }
    }
}
