using System.Net;

namespace Deuna.Merchant.Sdk.Exceptions;

/// <summary>
/// Thrown when the DEUNA API returns a 404 Not Found.
/// </summary>
public class DeunaNotFoundException : DeunaApiException
{
    public DeunaNotFoundException(string message, string? rawResponse = null)
        : base(HttpStatusCode.NotFound, message, rawResponse)
    {
    }
}
