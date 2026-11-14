namespace FpvBattle.Application.Features.Auth.Services;

public interface IGoogleTokenValidator
{
    /// <summary>
    /// Returns the identity in a valid Google ID token, or null when the token is invalid.
    /// </summary>
    Task<GoogleIdentity?> ValidateAsync(string idToken);
}

public record GoogleIdentity(string Subject, string Email, bool EmailVerified, string? Name, string? Locale);
