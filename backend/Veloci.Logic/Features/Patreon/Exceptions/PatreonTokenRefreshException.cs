namespace Veloci.Logic.Features.Patreon.Exceptions;

/// <summary>
///     Exception thrown when refreshing an expired Patreon access token fails.
/// </summary>
public class PatreonTokenRefreshException : PatreonAuthenticationException
{
    public PatreonTokenRefreshException(string message) : base(message)
    {
        TokenRefreshAttempted = true;
    }

    public PatreonTokenRefreshException(string message, Exception innerException) : base(message, innerException)
    {
        TokenRefreshAttempted = true;
    }

    /// <summary>The refresh token that failed to refresh the access token</summary>
    public string? RefreshToken { get; init; }
}
