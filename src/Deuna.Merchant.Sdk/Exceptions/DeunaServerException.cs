using System.Net;

namespace Deuna.Merchant.Sdk.Exceptions;

/// <summary>
/// Thrown when the DEUNA API returns a 500 Internal Server Error or other 5xx response.
/// </summary>
public class DeunaServerException : DeunaApiException
{
    public DeunaServerException(HttpStatusCode statusCode, string message, string? rawResponse = null)
        : base(statusCode, message, rawResponse)
    {
    }
}
