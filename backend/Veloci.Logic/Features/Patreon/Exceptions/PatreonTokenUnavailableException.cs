namespace Veloci.Logic.Features.Patreon.Exceptions;

/// <summary>
///     Exception thrown when no Patreon tokens are configured in database or configuration.
/// </summary>
public class PatreonTokenUnavailableException : PatreonAuthenticationException
{
    public PatreonTokenUnavailableException(string message) : base(message)
    {
    }

    public PatreonTokenUnavailableException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
