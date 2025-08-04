using System.Net;

namespace Veloci.Logic.Features.Patreon.Exceptions;

/// <summary>
///     Base exception for all Patreon-related errors, providing common properties for API context.
/// </summary>
public abstract class PatreonException : Exception
{
    protected PatreonException(string message) : base(message)
    {
    }

    protected PatreonException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>API endpoint that caused the error</summary>
    public string? Endpoint { get; init; }

    /// <summary>HTTP status code from the failed request</summary>
    public HttpStatusCode? StatusCode { get; init; }

    /// <summary>Raw response content from the API for debugging</summary>
    public string? ResponseContent { get; init; }
}
