namespace Veloci.Logic.Features.Patreon.Exceptions;

/// <summary>
/// Exception thrown when Patreon API authentication fails, including token expiration and refresh failures.
/// </summary>
public class PatreonAuthenticationException : PatreonException
{
    /// <summary>Indicates whether an automatic token refresh was attempted before failing</summary>
    public bool TokenRefreshAttempted { get; init; }

    public PatreonAuthenticationException(string message) : base(message)
    {
    }

    public PatreonAuthenticationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}