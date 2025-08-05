namespace Veloci.Logic.Features.Patreon.Exceptions;

/// <summary>
///     Exception thrown when Patreon API rate limits are exceeded.
/// </summary>
public class PatreonRateLimitException : PatreonException
{
    public PatreonRateLimitException(string message) : base(message)
    {
    }

    public PatreonRateLimitException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>Time to wait before retrying the request, if provided by the API</summary>
    public TimeSpan? RetryAfter { get; init; }
}
