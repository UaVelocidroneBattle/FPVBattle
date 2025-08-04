namespace Veloci.Logic.Features.Patreon.Exceptions;

/// <summary>
///     Exception thrown for general Patreon API errors that don't fall into specific categories.
/// </summary>
public class PatreonApiException : PatreonException
{
    public PatreonApiException(string message) : base(message)
    {
    }

    public PatreonApiException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
