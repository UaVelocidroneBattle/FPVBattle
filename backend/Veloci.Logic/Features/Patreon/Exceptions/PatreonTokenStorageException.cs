namespace Veloci.Logic.Features.Patreon.Exceptions;

/// <summary>
///     Exception thrown when storing or updating Patreon tokens in the database fails.
/// </summary>
public class PatreonTokenStorageException : PatreonException
{
    public PatreonTokenStorageException(string message) : base(message)
    {
    }

    public PatreonTokenStorageException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>Whether this was an update operation (true) or insert operation (false)</summary>
    public bool IsUpdate { get; init; }
}
