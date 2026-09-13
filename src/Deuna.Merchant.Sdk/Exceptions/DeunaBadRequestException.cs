using System.Net;

namespace Deuna.Merchant.Sdk.Exceptions;

/// <summary>
/// Thrown when the DEUNA API returns a 400 Bad Request.
/// </summary>
public class DeunaBadRequestException : DeunaApiException
{
    public DeunaBadRequestException(string message, string? rawResponse = null)
        : base(HttpStatusCode.BadRequest, message, rawResponse)
    {
    }
}
