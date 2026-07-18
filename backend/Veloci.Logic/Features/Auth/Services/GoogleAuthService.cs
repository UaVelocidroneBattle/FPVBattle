using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Auth.Models;

namespace Veloci.Logic.Features.Auth.Services;

/// <summary>
/// Validates Google ID tokens and resolves them to application users,
/// creating the user on first sign-in.
/// </summary>
public class GoogleAuthService
{
    public const string ProviderName = "Google";

    private static readonly ILogger Log = Serilog.Log.ForContext<GoogleAuthService>();

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOptions<AuthOptions> _options;

    public GoogleAuthService(UserManager<ApplicationUser> userManager, IOptions<AuthOptions> options)
    {
        _userManager = userManager;
        _options = options;
    }

    /// <summary>
    /// Returns the user for a valid Google ID token, or null when the token is invalid.
    /// </summary>
    public async Task<ApplicationUser?> AuthenticateAsync(string idToken)
    {
        var payload = await ValidateIdTokenAsync(idToken);

        if (payload is null)
            return null;

        var user = await _userManager.FindByLoginAsync(ProviderName, payload.Subject);

        if (user is not null)
        {
            await SyncProfileAsync(user, payload);
            return user;
        }

        return await CreateOrLinkUserAsync(payload);
    }

    private async Task<GoogleJsonWebSignature.Payload?> ValidateIdTokenAsync(string idToken)
    {
        try
        {
            return await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = _options.Value.GoogleClientIds
            });
        }
        catch (InvalidJwtException ex)
        {
            Log.Warning(ex, "Rejected invalid Google ID token");
            return null;
        }
    }

    /// <summary>
    /// Keeps the profile in sync with Google, so renames there show up here on next sign-in.
    /// </summary>
    private async Task SyncProfileAsync(ApplicationUser user, GoogleJsonWebSignature.Payload payload)
    {
        var displayName = payload.Name ?? user.DisplayName;
        var locale = payload.Locale ?? user.Locale;

        if (user.DisplayName == displayName && user.Locale == locale)
            return;

        user.DisplayName = displayName;
        user.Locale = locale;
        await _userManager.UpdateAsync(user);
    }

    private async Task<ApplicationUser?> CreateOrLinkUserAsync(GoogleJsonWebSignature.Payload payload)
    {
        // Only trust the email for matching an existing account when Google verified it
        var user = payload.EmailVerified
            ? await _userManager.FindByEmailAsync(payload.Email)
            : null;

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = payload.Email,
                Email = payload.Email,
                EmailConfirmed = payload.EmailVerified,
                DisplayName = payload.Name,
                Locale = payload.Locale
            };

            var creation = await _userManager.CreateAsync(user);

            if (!creation.Succeeded)
            {
                Log.Error("Failed to create user for Google account {Subject}: {Errors}",
                    payload.Subject, string.Join("; ", creation.Errors.Select(e => e.Description)));
                return null;
            }
        }

        var linking = await _userManager.AddLoginAsync(user, new UserLoginInfo(ProviderName, payload.Subject, ProviderName));

        if (!linking.Succeeded)
        {
            Log.Error("Failed to link Google login to user {UserId}: {Errors}",
                user.Id, string.Join("; ", linking.Errors.Select(e => e.Description)));
            return null;
        }

        Log.Information("Signed up user {UserId} via Google", user.Id);
        return user;
    }
}
