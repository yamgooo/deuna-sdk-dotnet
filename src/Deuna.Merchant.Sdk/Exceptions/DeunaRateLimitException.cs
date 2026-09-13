using System.Net;

namespace Deuna.Merchant.Sdk.Exceptions;

/// <summary>
/// Thrown when the DEUNA API returns a 429 Too Many Requests.
/// </summary>
public class DeunaRateLimitException : DeunaApiException
{
    public DeunaRateLimitException(string message, string? rawResponse = null)
        : base(HttpStatusCode.TooManyRequests, message, rawResponse)
    {
    }
}
