namespace Veloci.Logic.Features.Patreon.Exceptions;

/// <summary>
/// Exception thrown when parsing Patreon member data fails due to unexpected or invalid API response structure.
/// </summary>
public class PatreonMemberParsingException : PatreonException
{
    /// <summary>ID of the member that failed to parse, if available</summary>
    public string? MemberId { get; init; }

    public PatreonMemberParsingException(string message) : base(message)
    {
    }

    public PatreonMemberParsingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}