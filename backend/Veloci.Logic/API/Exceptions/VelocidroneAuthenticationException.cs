using System.Net;

namespace Veloci.Logic.API.Exceptions;

public class VelocidroneAuthenticationException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? ResponseContent { get; }
    public string Endpoint { get; }

    public VelocidroneAuthenticationException(string endpoint, HttpStatusCode statusCode, string? responseContent)
        : base($"Velocidrone API authentication failed for endpoint '{endpoint}'. Received HTML login page, indicating invalid credentials.")
    {
        Endpoint = endpoint;
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }

    public VelocidroneAuthenticationException(string endpoint, HttpStatusCode statusCode, string? responseContent, Exception innerException)
        : base($"Velocidrone API authentication failed for endpoint '{endpoint}'. Received HTML login page, indicating invalid credentials.", innerException)
    {
        Endpoint = endpoint;
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }
}