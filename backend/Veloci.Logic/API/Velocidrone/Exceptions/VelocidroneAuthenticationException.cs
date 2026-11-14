using System.Net;

namespace Veloci.Logic.API.Velocidrone.Exceptions;

public class VelocidroneAuthenticationException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? ResponseContent { get; }
    public string Endpoint { get; }

    public VelocidroneAuthenticationException(string endpoint, HttpStatusCode statusCode, string? responseContent)
        : base($"Velocidrone API authentication failed for endpoint '{endpoint}' ({(int)statusCode}). Check the API token.")
    {
        Endpoint = endpoint;
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }
}
