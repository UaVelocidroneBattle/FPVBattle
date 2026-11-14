namespace Veloci.Logic.API.Velocidrone.Exceptions;

public class VelocidroneApiException : Exception
{
    public string Endpoint { get; }

    public VelocidroneApiException(string endpoint, string? reason)
        : base($"Velocidrone API {endpoint} returned an unsuccessful response: {reason ?? "no message"}")
    {
        Endpoint = endpoint;
    }
}
